using Microsoft.AspNetCore.Http;

namespace Bite.Api.Dtos;

public class RegisterRestaurantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string WebhookUrl { get; set; } = string.Empty;
    public string? OpeningHoursJson { get; set; } 
    public IFormFile? CoverImage { get; set; }
}