using Bite.Api.Dtos;
using Bite.Api.Dtos.Mappers;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Bite.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantsController(IRestaurantService restaurantService, 
        IWebHostEnvironment env) : ControllerBase
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

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] RegisterRestaurantRequest request, CancellationToken cancellationToken)
        {
            var address = new Address(
                id: 0,
                street: request.Street,
                number: request.Number,
                zipCode: request.ZipCode,
                city: request.City,
                country: request.Country,
                longitude: request.Longitude,
                latitude: request.Latitude
            );

            var restaurant = new Restaurant(
                id: 0,
                name: request.Name,
                addressId: 0,
                webhookUrl: request.WebhookUrl,
                apiKey: string.Empty
            );

            var openingHours = new List<OpeningHourSlot>();
            if (!string.IsNullOrEmpty(request.OpeningHoursJson))
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dtos = JsonSerializer.Deserialize<List<OpeningHourSlotDto>>(request.OpeningHoursJson, options);
                if (dtos != null)
                {
                    foreach (var dto in dtos)
                    {
                        openingHours.Add(new OpeningHourSlot(
                            id: 0,
                            restaurantId: 0,
                            dayOfWeek: dto.DayOfWeek,
                            openTime: dto.OpenTime,
                            closeTime: dto.CloseTime
                        ));
                    }
                }
            }

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
                return result.ResultType switch
                {
                    ServiceResultType.Conflict => Conflict(new { message = result.ErrorMessage }),
                    _ => BadRequest(new { message = result.ErrorMessage })
                };
            }

            var (restaurantId, rawApiKey) = result.Data;

            return CreatedAtAction(null, new RegisterRestaurantResponse
            {
                RestaurantId = restaurantId,
                ApiKey = rawApiKey
            });
        }
    }
}
