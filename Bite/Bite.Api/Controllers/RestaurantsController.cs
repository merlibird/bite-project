using System.ComponentModel.DataAnnotations;
using Bite.Api.Dtos;
using Bite.Api.Dtos.Mappers;
using Bite.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Bite.Api.Controllers;

[ApiController]
[Route("api/restaurants")]
public class RestaurantsController(IRestaurantService restaurantService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<RestaurantSearchResultDto>> SearchRestaurants(
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

        return Ok(restaurants.ToRestaurantSearchResultDto(latitude, longitude, openNow, count));
    }
}
