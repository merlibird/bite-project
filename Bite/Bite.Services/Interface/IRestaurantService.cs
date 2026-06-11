using Bite.Domain;

namespace Bite.Services.Interface;

public interface IRestaurantService
{
    Task<IReadOnlyCollection<(
        Restaurant Restaurant,
        Address Address,
        double DistanceInKm,
        bool IsOpenNow,
        IReadOnlyCollection<OpeningHourSlot> OpeningHours)>> SearchRestaurantsAsync(
        double latitude,
        double longitude,
        bool openNowOnly,
        int count,
        CancellationToken cancellationToken = default);
}
