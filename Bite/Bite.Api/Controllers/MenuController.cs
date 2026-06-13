using Bite.Api.Dtos;
using Bite.Api.Dtos.Mappers;
using Bite.Services.Common;
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

    [HttpPut("{id:int}/menu")]
    public async Task<ActionResult<MenuDto>> UpdateMenu(
        [FromRoute] int id,
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromBody] UpdateMenuRequest request,
        CancellationToken cancellationToken)
    {
        var result = await menuService.UpdateMenuAsync(
            id,
            request.ToMenu(id),
            apiKey ?? string.Empty,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ServiceResultType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(result.Data!.ToMenuDto());
    }
}
