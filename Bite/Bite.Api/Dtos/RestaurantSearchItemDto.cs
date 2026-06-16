namespace Bite.Api.Dtos;

public record RestaurantSearchItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? TitleImagePath { get; init; }
    public RestaurantAddressDto Address { get; init; } = new();
    public double DistanceInKm { get; init; }
    public bool IsOpenNow { get; init; }
    public List<OpeningHourSlotDto> OpeningHours { get; init; } = [];
}
