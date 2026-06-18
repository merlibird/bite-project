using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Interface;
using System.Transactions;
using Microsoft.AspNetCore.Hosting;

namespace Bite.Services.Implementation;

public class RestaurantService(
    IRestaurantDao restaurantDao,
    IAddressDao addressDao,
    IOpeningHourSlotDao openingHourSlotDao,
    IApiKeyService apiKeyService,
    TimeProvider timeProvider,
    IDeliveryZoneDao deliveryZoneDao,
    IDeliveryFeeRuleDao deliveryFeeRuleDao) : IRestaurantService
{
    //private const string ImageBaseDir = "wwwroot/images/restaurants";

    public async Task<ServiceResult<bool>> UpdateDeliveryConditionsAsync(
        int restaurantId,
        IEnumerable<DeliveryZone> deliveryZones,
        IEnumerable<DeliveryFeeRule> feeRules,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await restaurantDao.FindByIdAsync(restaurantId, cancellationToken);
        if (restaurant is null)
        {
            return ServiceResult<bool>.Failure("Restaurant not found.", ServiceResultType.NotFound);
        }

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Delete existing rules and zones
        await deliveryFeeRuleDao.DeleteAllByRestaurantIdAsync(restaurantId, cancellationToken);
        await deliveryZoneDao.DeleteAllByRestaurantIdAsync(restaurantId, cancellationToken);

        // Insert new zones and rules
        foreach (var zone in deliveryZones)
        {
            var zoneToInsert = new DeliveryZone(
                id: 0,
                restaurantId: restaurantId,
                minOrderValue: zone.MinOrderValue,
                maxDistance: zone.MaxDistance
            );

            int zoneId = await deliveryZoneDao.InsertAsync(zoneToInsert, cancellationToken);

            var zoneRules = feeRules.Where(r => r.DeliveryZoneId == zone.Id); // Mapping based on temporary IDs from DTO conversion
            foreach (var rule in zoneRules)
            {
                var ruleToInsert = new DeliveryFeeRule(
                    id: 0,
                    deliveryZoneId: zoneId,
                    maxOrderValue: rule.MaxOrderValue,
                    deliveryFee: rule.DeliveryFee
                );
                await deliveryFeeRuleDao.InsertAsync(ruleToInsert, cancellationToken);
            }
        }

        scope.Complete();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<(int RestaurantId, string RawApiKey)>> RegisterAsync(
        Restaurant restaurant,
        Address address,
        List<OpeningHourSlot> openingHours,
        Stream? imageStream,
        string? imageExtension,
        string webRootPath,
        CancellationToken cancellationToken = default)
    {
        var existing = await restaurantDao.FindByNameAndCityAsync(restaurant.Name, address.City, cancellationToken);
        if (existing != null)
        {
            return ServiceResult<(int, string)>.Failure(
                $"Restaurant '{restaurant.Name}' in city '{address.City}' already exists.",
                ServiceResultType.Conflict);
        }

        string? imagePath = null;
        if (imageStream != null && !string.IsNullOrEmpty(imageExtension))
        {
            if (imageStream.Length < 10)
            {
                return ServiceResult<(int, string)>.Failure(
                    $"The uploaded image is too small or invalid ({imageStream.Length} bytes received).",
                    ServiceResultType.Error);
            }

            var fileName = $"{Guid.NewGuid()}{imageExtension}";
            var fullPath = Path.Combine(webRootPath, "images", "restaurants", fileName);

            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var fileStream = new FileStream(fullPath, FileMode.Create);
            await imageStream.CopyToAsync(fileStream, cancellationToken);
            imagePath = $"/images/restaurants/{fileName}";
        }

        string rawApiKey = apiKeyService.GenerateApiKey();
        string hashedApiKey = apiKeyService.HashApiKey(rawApiKey);

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // 1. Insert Address
        int addressId = await addressDao.InsertAsync(address, cancellationToken);

        // 2. Insert Restaurant
        var restaurantToInsert = new Restaurant(
            id: 0,
            name: restaurant.Name,
            addressId: addressId,
            webhookUrl: restaurant.WebhookUrl,
            apiKey: hashedApiKey,
            titleImagePath: imagePath
        );

        int restaurantId = await restaurantDao.InsertAsync(restaurantToInsert, cancellationToken);

        // 3. Insert Opening Hours
        foreach (var slot in openingHours)
        {
            var slotToInsert = new OpeningHourSlot(
                id: 0,
                restaurantId: restaurantId,
                dayOfWeek: slot.DayOfWeek,
                openTime: slot.OpenTime,
                closeTime: slot.CloseTime
            );
            await openingHourSlotDao.InsertAsync(slotToInsert, cancellationToken);
        }

        scope.Complete();

        return ServiceResult<(int, string)>.Success((restaurantId, rawApiKey));
    }

    public async Task<IReadOnlyCollection<(
        Restaurant Restaurant,
        Address Address,
        double DistanceInKm,
        bool IsOpenNow,
        IReadOnlyCollection<OpeningHourSlot> OpeningHours)>> SearchRestaurantsAsync(
            double latitude,
            double longitude,
            bool openNowOnly,
            int count,
            CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetLocalNow().DateTime;

        var restaurants = await restaurantDao.FindAllAsync(cancellationToken);
        var addressesById = (await addressDao.FindAllAsync(cancellationToken))
            .ToDictionary(a => a.Id);
        var openingHoursByRestaurant = (await openingHourSlotDao.FindAllAsync(cancellationToken))
            .GroupBy(h => h.RestaurantId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var deliveryZonesByRestaurant = (await deliveryZoneDao.FindAllAsync(cancellationToken))
            .GroupBy(z => z.RestaurantId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var searchItems = new List<(
            Restaurant Restaurant,
            Address Address,
            double DistanceInKm,
            bool IsOpenNow,
            IReadOnlyCollection<OpeningHourSlot> OpeningHours)>();

        foreach (var restaurant in restaurants)
        {
            if (!addressesById.TryGetValue(restaurant.AddressId, out var address))
            {
                continue;
            }

            var openingHours = openingHoursByRestaurant.GetValueOrDefault(restaurant.Id, []);
            var isOpenNow = IsOpenAt(openingHours, now);

            if (openNowOnly && !isOpenNow)
            {
                continue;
            }

            var distanceInKm = GeoUtils.CalculateDistanceInKm(
                latitude,
                longitude,
                address.Latitude,
                address.Longitude);

            // Check if user is within any delivery zone of the restaurant
            var deliveryZones = deliveryZonesByRestaurant.GetValueOrDefault(restaurant.Id, []);
            if (!deliveryZones.Any(z => distanceInKm <= z.MaxDistance))
            {
                continue;
            }

            searchItems.Add((
                restaurant,
                address,
                Math.Round(distanceInKm, 2),
                isOpenNow,
                openingHours));
        }

        var result = searchItems
            .OrderBy(item => item.DistanceInKm)
            .Take(count)
            .ToList();

        return result;
    }

    internal static bool IsOpenAt(IEnumerable<OpeningHourSlot> openingHours, DateTime dateTime)
    {
        var dayOfWeek = (int)dateTime.DayOfWeek;
        var time = dateTime.TimeOfDay;

        return openingHours.Any(slot =>
            IsOpenInSameDaySlot(slot, dayOfWeek, time) ||
            IsOpenInOvernightSlot(slot, dayOfWeek, time));
    }

    private static bool IsOpenInSameDaySlot(OpeningHourSlot slot, int dayOfWeek, TimeSpan time)
    {
        return slot.DayOfWeek == dayOfWeek &&
               slot.OpenTime <= slot.CloseTime &&
               slot.OpenTime <= time &&
               time < slot.CloseTime;
    }

    private static bool IsOpenInOvernightSlot(OpeningHourSlot slot, int dayOfWeek, TimeSpan time)
    {
        if (slot.OpenTime <= slot.CloseTime)
        {
            return false;
        }

        var previousDay = (dayOfWeek + 6) % 7;
        return (slot.DayOfWeek == dayOfWeek && time >= slot.OpenTime) ||
               (slot.DayOfWeek == previousDay && time < slot.CloseTime);
    }
}