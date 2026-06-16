using Bite.Api.Auth;
using Bite.Api.Dtos;
using Bite.Api.Dtos.Mappers;
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

    [ApiKeyAuth]
    [HttpGet("{orderCode}/status-change/{token}")]
    public async Task<IActionResult> ApplyStatusToken(
        [FromRoute] string orderCode,
        [FromRoute] string token,
        CancellationToken cancellationToken)
    {
        int restaurantId = (int)HttpContext.Items[ApiKeyAuthAttribute.RestaurantIdItem]!;

        var result = await orderService.ApplyStatusTokenAsync(orderCode, token, restaurantId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.NotFound or ServiceResultType.Forbidden => NotFound(new { message = "No matching order or token found." }),
                ServiceResultType.Conflict => Conflict(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(new OrderStatusResponse
        {
            OrderCode = orderCode,
            Status = result.Data.ToDbValue()
        });
    }

    [HttpPost("/api/restaurants/{restaurantId}/orders/price")]
    public async Task<IActionResult> CalculatePrice(
        [FromRoute] int restaurantId,
        [FromBody] OrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await orderService.CalculatePriceAsync(
            restaurantId,
            request.Items.ToItemTuples(),
            (request.DeliveryAddress.Latitude, request.DeliveryAddress.Longitude),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ServiceResultType.ValidationError => UnprocessableEntity(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(new Dtos.OrderPriceResponse
        {
            Subtotal = result.Data!.Subtotal,
            DeliveryFee = result.Data!.DeliveryFee
        });
    }

    [HttpPost("/api/restaurants/{restaurantId}/orders")]
    public async Task<IActionResult> PlaceOrder(
        [FromRoute] int restaurantId,
        [FromBody] OrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await orderService.PlaceOrderAsync(
            restaurantId,
            request.Items.ToItemTuples(),
            request.DeliveryAddress.ToDomain(),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ServiceResultType.ValidationError => UnprocessableEntity(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return CreatedAtAction(nameof(GetStatus), new { orderCode = result.Data }, new PlaceOrderResponse
        {
            OrderCode = result.Data!
        });
    }
}
