namespace Bite.Api.Dtos;

public record RestaurantAddressDto
{
    public string Street { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string? AdditionalInfo { get; init; }
}
