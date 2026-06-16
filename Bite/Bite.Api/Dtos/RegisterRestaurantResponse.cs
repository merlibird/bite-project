namespace Bite.Api.Dtos;

public record RegisterRestaurantResponse
{
    public int RestaurantId { get; init; }
    public string ApiKey { get; init; } = string.Empty;
}
