using Bite.Api.Dtos;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Bite.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpGet("{orderCode}/status")]
    public async Task<IActionResult> GetStatus([FromRoute] string orderCode, CancellationToken cancellationToken)
    {
        var result = await orderService.GetStatusAsync(orderCode, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
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
