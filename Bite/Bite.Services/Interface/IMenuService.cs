using Bite.Domain;
using Bite.Services.Common;

namespace Bite.Services.Interface;

public interface IMenuService
{
    Task<ServiceResult<Menu>> GetMenuAsync(int restaurantId, CancellationToken cancellationToken = default);

    Task<ServiceResult<Menu>> UpdateMenuAsync(
        int restaurantId,
        Menu menu,
        CancellationToken cancellationToken = default);
}
