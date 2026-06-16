using Bite.Api.Auth;
using Bite.Api.Dtos;
using Bite.Api.Dtos.Mappers;
using Bite.Services.Common;
using Bite.Services.Interface;
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
        var menu = await menuService.GetMenuAsync(restaurantId, cancellationToken);

        if (menu is null)
        {
            return NotFound(new { message = "Restaurant not found." });
        }

        return Ok(menu.ToMenuResponse());
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
            return Forbid();
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
