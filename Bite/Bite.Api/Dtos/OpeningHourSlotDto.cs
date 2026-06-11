namespace Bite.Api.Dtos;

public record OpeningHourSlotDto
{
    public int DayOfWeek { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
}
