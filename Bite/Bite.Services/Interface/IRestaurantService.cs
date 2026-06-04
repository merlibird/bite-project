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
}
