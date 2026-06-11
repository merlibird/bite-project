using Bite.Domain;
<<<<<<< HEAD
using Bite.Services.Common;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
=======
>>>>>>> feature/issue-48-get-restaurants

namespace Bite.Services.Interface;

public interface IRestaurantService
{
<<<<<<< HEAD
    Task<ServiceResult<(int RestaurantId, string RawApiKey)>> RegisterAsync(
        Restaurant restaurant, 
        Address address, 
        List<OpeningHourSlot> openingHours, 
        Stream? imageStream, 
        string? imageExtension,
        string webRootPath,
=======
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
>>>>>>> feature/issue-48-get-restaurants
        CancellationToken cancellationToken = default);
}
