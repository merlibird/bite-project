using System.ComponentModel.DataAnnotations;

namespace Bite.Api.Dtos;

public record RestaurantAddressDto
{
    [Required]
    [StringLength(255)]
    public string Street { get; init; } = string.Empty;

    [StringLength(50)]
    public string Number { get; init; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ZipCode { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; init; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; init; }

    [Range(-180, 180)]
    public double Longitude { get; init; }

    [StringLength(255)]
    public string? AdditionalInfo { get; init; }
}
