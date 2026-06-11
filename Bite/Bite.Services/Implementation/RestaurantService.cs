using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Interface;

namespace Bite.Services.Implementation;

public class RestaurantService(
    IRestaurantDao restaurantDao,
    IAddressDao addressDao,
    IOpeningHourSlotDao openingHourSlotDao,
    TimeProvider timeProvider) : IRestaurantService
{
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
        var searchItems = new List<(
            Restaurant Restaurant,
            Address Address,
            double DistanceInKm,
            bool IsOpenNow,
            IReadOnlyCollection<OpeningHourSlot> OpeningHours)>();

        foreach (var restaurant in restaurants)
        {
            var address = await addressDao.FindByIdAsync(restaurant.AddressId, cancellationToken);
            if (address is null)
            {
                continue;
            }

            var openingHours = (await openingHourSlotDao.FindByRestaurantIdAsync(restaurant.Id, cancellationToken)).ToList();
            var isOpenNow = IsOpenAt(openingHours, now);

            if (openNowOnly && !isOpenNow)
            {
                continue;
            }

            var distanceInKm = CalculateDistanceInKm(
                latitude,
                longitude,
                address.Latitude,
                address.Longitude);

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

    private static double CalculateDistanceInKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        const double earthRadiusInKm = 6371;

        var latitudeDistance = ToRadians(latitude2 - latitude1);
        var longitudeDistance = ToRadians(longitude2 - longitude1);
        var currentLatitude = ToRadians(latitude1);
        var restaurantLatitude = ToRadians(latitude2);

        var a = Math.Sin(latitudeDistance / 2) * Math.Sin(latitudeDistance / 2) +
                Math.Cos(currentLatitude) * Math.Cos(restaurantLatitude) *
                Math.Sin(longitudeDistance / 2) * Math.Sin(longitudeDistance / 2);

        var c = 2 * Math.Asin(Math.Sqrt(a));

        return earthRadiusInKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
