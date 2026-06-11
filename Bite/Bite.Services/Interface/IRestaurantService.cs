using Bite.Domain;
using Bite.Services.Common;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bite.Services.Interface;

public interface IRestaurantService
{
    Task<ServiceResult<(int RestaurantId, string RawApiKey)>> RegisterAsync(
        Restaurant restaurant,
        Address address,
        List<OpeningHourSlot> openingHours,
        Stream? imageStream,
        string? imageExtension,
        string webRootPath,
        CancellationToken cancellationToken = default);

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