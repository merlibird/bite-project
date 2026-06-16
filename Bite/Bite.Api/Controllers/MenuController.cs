using Bite.Api.Auth;
using Bite.Api.Dtos;
using Bite.Api.Dtos.Mappers;
using Bite.Services.Common;
using Bite.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bite.Api.Controllers;

[ApiController]
[Route("api/restaurants")]
public class MenuController(IMenuService menuService) : ApiControllerBase
{
    [HttpGet("{restaurantId:int}/menu")]
    public async Task<ActionResult<MenuResponse>> FindMenuByRestaurantId(
        [FromRoute] int restaurantId,
        CancellationToken cancellationToken)
    {
        var result = await menuService.GetMenuAsync(restaurantId, cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        return Ok(result.Data!.ToMenuResponse());
    }

    [ApiKeyAuth]
    [HttpPut("{id:int}/menu")]
    public async Task<ActionResult<MenuResponse>> UpdateMenu(
        [FromRoute] int id,
        [FromBody] UpdateMenuRequest request,
        CancellationToken cancellationToken)
    {
        int authenticatedRestaurantId = (int)HttpContext.Items[ApiKeyAuthAttribute.RestaurantIdItem]!;

        if (id != authenticatedRestaurantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only modify your own restaurant." });
        }

        var result = await menuService.UpdateMenuAsync(
            id,
            request.ToMenu(id),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        return Ok(result.Data!.ToMenuResponse());
    }
}
