namespace Bite.Api.Dtos;

public record RestaurantSearchResult
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public bool OpenNowOnly { get; init; }
    public int Count { get; init; }
    public List<RestaurantSearchItemDto> Restaurants { get; init; } = [];
}
