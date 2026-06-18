namespace Bite.Api.Dtos;

public record OpeningHourSlotDto
{
    public int DayOfWeek { get; init; }
    public TimeSpan OpenTime { get; init; }
    public TimeSpan CloseTime { get; init; }
}
