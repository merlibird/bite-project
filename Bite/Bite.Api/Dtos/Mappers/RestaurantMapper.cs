using Bite.Domain;
using System.Text.Json;

namespace Bite.Api.Dtos.Mappers;

public static class RestaurantMapper
{
    public static (Restaurant Restaurant, Address Address, List<OpeningHourSlot> OpeningHours) ToDomain(this RegisterRestaurantRequest request)
    {
        var address = new Address(
            id: 0,
            street: request.Street,
            number: request.Number,
            zipCode: request.ZipCode,
            city: request.City,
            country: request.Country,
            longitude: request.Longitude,
            latitude: request.Latitude
        );

        var restaurant = new Restaurant(
            id: 0,
            name: request.Name,
            addressId: 0,
            webhookUrl: request.WebhookUrl,
            apiKey: string.Empty
        );

        var openingHours = new List<OpeningHourSlot>();
        if (!string.IsNullOrEmpty(request.OpeningHoursJson))
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dtos = JsonSerializer.Deserialize<List<OpeningHourSlotDto>>(request.OpeningHoursJson, options);
            if (dtos != null)
            {
                foreach (var dto in dtos)
                {
                    openingHours.Add(new OpeningHourSlot(
                        id: 0,
                        restaurantId: 0,
                        dayOfWeek: dto.DayOfWeek,
                        openTime: dto.OpenTime,
                        closeTime: dto.CloseTime
                    ));
                }
            }
        }

        return (restaurant, address, openingHours);
    }

    public static (List<DeliveryZone> Zones, List<DeliveryFeeRule> Rules) ToDomain(this List<DeliveryZoneDto> request, int restaurantId)
    {
        var deliveryZones = new List<DeliveryZone>();
        var feeRules = new List<DeliveryFeeRule>();

        // Temporary ID to link zones and rules before DB insertion
        int tempZoneId = 1;

        foreach (var zoneDto in request)
        {
            deliveryZones.Add(new DeliveryZone(
                id: tempZoneId,
                restaurantId: restaurantId,
                minOrderValue: zoneDto.MinOrderValue,
                maxDistance: zoneDto.MaxDistance
            ));

            foreach (var ruleDto in zoneDto.FeeRules)
            {
                feeRules.Add(new DeliveryFeeRule(
                    id: 0,
                    deliveryZoneId: tempZoneId,
                    maxOrderValue: ruleDto.MaxOrderValue,
                    deliveryFee: ruleDto.DeliveryFee
                ));
            }
            tempZoneId++;
        }

        return (deliveryZones, feeRules);
    }

    public static RestaurantSearchResult ToRestaurantSearchResult(
        this IReadOnlyCollection<(
            Restaurant Restaurant,
            Address Address,
            double DistanceInKm,
            bool IsOpenNow,
            IReadOnlyCollection<OpeningHourSlot> OpeningHours)> restaurants,
        double latitude,
        double longitude,
        bool openNowOnly,
        int count)
    {
        return new RestaurantSearchResult
        {
            Latitude = latitude,
            Longitude = longitude,
            OpenNowOnly = openNowOnly,
            Count = count,
            Restaurants = restaurants
                .Select(item => new RestaurantSearchItemDto
                {
                    Id = item.Restaurant.Id,
                    Name = item.Restaurant.Name,
                    TitleImagePath = item.Restaurant.TitleImagePath,
                    Address = new RestaurantAddressDto
                    {
                        Street = item.Address.Street,
                        Number = item.Address.Number,
                        ZipCode = item.Address.ZipCode,
                        City = item.Address.City,
                        Country = item.Address.Country,
                        Latitude = item.Address.Latitude,
                        Longitude = item.Address.Longitude,
                        AdditionalInfo = item.Address.AdditionalInfo
                    },
                    DistanceInKm = item.DistanceInKm,
                    IsOpenNow = item.IsOpenNow,
                    OpeningHours = item.OpeningHours
                        .Select(slot => new OpeningHourSlotDto
                        {
                            DayOfWeek = slot.DayOfWeek,
                            OpenTime = slot.OpenTime,
                            CloseTime = slot.CloseTime
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
