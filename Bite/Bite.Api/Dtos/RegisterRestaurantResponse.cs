namespace Bite.Api.Dtos;

public class RegisterRestaurantResponse
{
    public int RestaurantId { get; set; }
    public string ApiKey { get; set; } = string.Empty;
}
