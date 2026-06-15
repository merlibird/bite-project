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

    //warum wollten wir das über restaurants haben?
    //Weil es einfacher ist, die Berechtigungen zu überprüfen,
    //wenn die Restaurant-ID in der URL enthalten ist? So können
    //wir sicherstellen, dass nur berechtigte Restaurants den Status
    //ihrer eigenen Bestellungen ändern können????
    [HttpPost("/api/restaurants/{restaurantId}/orders/price")]
    public async Task<IActionResult> CalculatePrice(
        [FromRoute] int restaurantId,
        [FromBody] OrderPriceRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await orderService.CalculatePriceAsync(
            restaurantId,
            request.Items.Select(i => (i.MenuItemId, i.Quantity)),
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

        return Ok(new OrderPriceResponseDto
        {
            Subtotal = result.Data!.Subtotal,
            DeliveryFee = result.Data!.DeliveryFee
        });
    }

    [HttpPost("/api/restaurants/{restaurantId}/orders")]
    public async Task<IActionResult> PlaceOrder(
        [FromRoute] int restaurantId,
        [FromBody] PlaceOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        var address = new Address(
            id: 0,
            street: request.DeliveryAddress.Street,
            number: request.DeliveryAddress.Number,
            zipCode: request.DeliveryAddress.ZipCode,
            city: request.DeliveryAddress.City,
            country: request.DeliveryAddress.Country,
            longitude: request.DeliveryAddress.Longitude,
            latitude: request.DeliveryAddress.Latitude,
            additionalInfo: request.DeliveryAddress.AdditionalInfo
        );

        var result = await orderService.PlaceOrderAsync(
            restaurantId,
            request.Items.Select(i => (i.MenuItemId, i.Quantity)),
            address,
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

        return CreatedAtAction(nameof(GetStatus), new { orderCode = result.Data }, new PlaceOrderResponseDto
        {
            OrderCode = result.Data!
        });
    }
}
