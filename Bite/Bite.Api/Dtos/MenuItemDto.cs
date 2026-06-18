namespace Bite.Api.Dtos;
public record MenuItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public bool IsActive { get; init; }
}
