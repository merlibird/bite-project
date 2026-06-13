using Bite.Api.Auth;
using Bite.Api.Dtos;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Bite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet("{orderCode}/status")]
    public async Task<IActionResult> GetStatus([FromRoute] string orderCode, CancellationToken cancellationToken)
    {
        var result = await orderService.GetStatusAsync(orderCode, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }), _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(new OrderStatusResponse
        {
            OrderCode = orderCode,
            Status = result.Data.ToDbValue()
        });
    }

    [ApiKeyAuth]
    [HttpPatch("{orderCode}")]
    public async Task<IActionResult> ChangeStatus(
        [FromRoute] string orderCode,
        [FromBody] ChangeOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        int restaurantId = (int)HttpContext.Items[ApiKeyAuthAttribute.RestaurantIdItem]!;

        if (!OrderStatusExtensions.IsValidOrderStatus(request.Status))
        {
            return BadRequest(new { message = $"Invalid status '{request.Status}'." });
        }

        var result = await orderService.ChangeStatusAsync(
            orderCode, 
            restaurantId, 
            OrderStatusExtensions.FromDbValue(request.Status), 
            cancellationToken
);

        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.NotFound or ServiceResultType.Forbidden => NotFound(new { message = $"No order found with code '{orderCode}'." }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(new OrderStatusResponse
        {
            OrderCode = orderCode,
            Status = result.Data.ToDbValue()
        });
    }
}
