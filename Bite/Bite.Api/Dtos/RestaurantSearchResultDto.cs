namespace Bite.Api.Dtos;

public record RestaurantSearchResultDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool OpenNowOnly { get; set; }
    public int Count { get; set; }
    public List<RestaurantSearchItemDto> Restaurants { get; set; } = [];
}
