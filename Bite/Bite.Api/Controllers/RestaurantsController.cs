using Bite.Api.Auth;
using Bite.Api.Dtos;
using Bite.Api.Dtos.Mappers;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController(IRestaurantService restaurantService,
    IWebHostEnvironment env) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<RestaurantSearchResult>> SearchRestaurants(
    [FromQuery, Range(-90, 90)] double latitude,
    [FromQuery, Range(-180, 180)] double longitude,
    [FromQuery] bool openNow = false,
    [FromQuery, Range(1, 100)] int count = 10,
    CancellationToken cancellationToken = default)
    {
        var restaurants = await restaurantService.SearchRestaurantsAsync(
            latitude,
            longitude,
            openNow,
            count,
            cancellationToken);

        return Ok(restaurants.ToRestaurantSearchResult(latitude, longitude, openNow, count));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Register([FromForm] RegisterRestaurantRequest request, CancellationToken cancellationToken)
    {
        var (restaurant, address, openingHours) = request.ToDomain();

        Stream? imageStream = request.CoverImage?.OpenReadStream();
        if (imageStream != null && imageStream.CanSeek)
        {
            imageStream.Position = 0;
        }
        string? imageExtension = request.CoverImage != null ? Path.GetExtension(request.CoverImage.FileName) : null;

        var result = await restaurantService.RegisterAsync(
            restaurant,
            address,
            openingHours,
            imageStream,
            imageExtension,
            env.WebRootPath,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        var (restaurantId, rawApiKey) = result.Data;

        return CreatedAtAction(null, new RegisterRestaurantResponse
        {
            RestaurantId = restaurantId,
            ApiKey = rawApiKey
        });
    }

    [ApiKeyAuth]
    [HttpPut("{id}/delivery-conditions")]
    public async Task<IActionResult> UpdateDeliveryConditions(
        int id,
        [FromBody] List<DeliveryZoneDto> request,
        CancellationToken cancellationToken)
    {
        int authenticatedRestaurantId = (int)HttpContext.Items[ApiKeyAuthAttribute.RestaurantIdItem]!;

        if (id != authenticatedRestaurantId)
        {
            return Forbid();
        }

        var (deliveryZones, feeRules) = request.ToDomain(id);

        var result = await restaurantService.UpdateDeliveryConditionsAsync(
            id,
            deliveryZones,
            feeRules,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
