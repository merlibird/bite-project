namespace Bite.Api.Dtos;

public record RestaurantSearchItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TitleImagePath { get; set; }
    public RestaurantAddressDto Address { get; set; } = new();
    public double DistanceInKm { get; set; }
    public bool IsOpenNow { get; set; }
    public List<OpeningHourSlotDto> OpeningHours { get; set; } = [];
}
