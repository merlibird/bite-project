using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Bite.Api.Dtos;

public class RegisterRestaurantRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Street { get; set; } = string.Empty;

    [StringLength(50)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ZipCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Url]
    [StringLength(255)]
    public string WebhookUrl { get; set; } = string.Empty;

    public string? OpeningHoursJson { get; set; }
    public IFormFile? CoverImage { get; set; }
}