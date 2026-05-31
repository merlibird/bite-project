using Bite.Api.Dtos;
using Bite.Api.Dtos.Mappers;
using Bite.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Bite.Api.Controllers;

[ApiController]
[Route("api/restaurants")]
public class MenuController(IMenuService menuService) : ControllerBase
{
    [HttpGet("{restaurantId:int}/menu")]
    public async Task<ActionResult<MenuDto>> FindMenuByRestaurantId(
        [FromRoute] int restaurantId,
        CancellationToken cancellationToken)
    {
        var menu = await menuService.GetMenuAsync(restaurantId, cancellationToken);

        if (menu is null)
        {
            return NotFound();
        }

        return Ok(menu.ToMenuDto());
    }
}