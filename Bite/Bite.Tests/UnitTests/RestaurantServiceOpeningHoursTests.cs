using Bite.Domain;
using Bite.Services.Implementation;

// This test class was created with the help and assistance of AI 
namespace Bite.Tests.UnitTests;

public class RestaurantServiceOpeningHoursTests
{
    // DayOfWeek convention: 0 = Sunday, 1 = Monday, ... 6 = Saturday.
    private static OpeningHourSlot Slot(int day, string open, string close)
        => new(0, 0, day, TimeSpan.Parse(open), TimeSpan.Parse(close));

    private static DateTime Wednesday(string time) // 2026-06-17, DayOfWeek = 3
        => new DateOnly(2026, 6, 17).ToDateTime(TimeOnly.Parse(time));

    private static DateTime Thursday(string time)  // 2026-06-18, DayOfWeek = 4
        => new DateOnly(2026, 6, 18).ToDateTime(TimeOnly.Parse(time));

    private static bool IsOpen(DateTime when, params OpeningHourSlot[] slots)
        => RestaurantService.IsOpenAt(slots, when);

    [Fact]
    public void IsOpenAt_NoSlots_ReturnsClosed()
        => Assert.False(IsOpen(Wednesday("12:00")));

    // =====================================================================
    // Same-day slot
    // =====================================================================

    [Fact]
    public void IsOpenAt_WithinSameDaySlot_ReturnsOpen()
        => Assert.True(IsOpen(Wednesday("12:00"), Slot(3, "09:00", "17:00")));

    [Fact]
    public void IsOpenAt_AtOpeningTime_ReturnsOpen() // lower bound inclusive
        => Assert.True(IsOpen(Wednesday("09:00"), Slot(3, "09:00", "17:00")));

    [Fact]
    public void IsOpenAt_AtClosingTime_ReturnsClosed() // upper bound exclusive
        => Assert.False(IsOpen(Wednesday("17:00"), Slot(3, "09:00", "17:00")));

    [Fact]
    public void IsOpenAt_BeforeOpening_ReturnsClosed()
        => Assert.False(IsOpen(Wednesday("08:59"), Slot(3, "09:00", "17:00")));

    [Fact]
    public void IsOpenAt_SlotOnDifferentDay_ReturnsClosed()
        => Assert.False(IsOpen(Wednesday("12:00"), Slot(1, "09:00", "17:00"))); // Monday only

    [Fact]
    public void IsOpenAt_DuringPauseBetweenTwoSlots_ReturnsClosed()
        => Assert.False(IsOpen(Wednesday("16:00"), Slot(3, "11:00", "15:00"), Slot(3, "17:00", "22:00")));

    [Fact]
    public void IsOpenAt_InFirstSlotBeforePause_ReturnsOpen()
        => Assert.True(IsOpen(Wednesday("12:00"), Slot(3, "11:00", "15:00"), Slot(3, "17:00", "22:00")));

    [Fact]
    public void IsOpenAt_InSecondSlotAfterPause_ReturnsOpen()
        => Assert.True(IsOpen(Wednesday("18:00"), Slot(3, "11:00", "15:00"), Slot(3, "17:00", "22:00")));

    // =====================================================================
    // Overnight slot (e.g. Wed 22:00 -> Thu 02:00)
    // =====================================================================

    [Fact]
    public void IsOpenAt_OvernightSlot_LateEveningOnOpeningDay_ReturnsOpen()
        => Assert.True(IsOpen(Wednesday("23:30"), Slot(3, "22:00", "02:00")));

    [Fact]
    public void IsOpenAt_OvernightSlot_AfterMidnightOnNextDay_ReturnsOpen()
        => Assert.True(IsOpen(Thursday("01:00"), Slot(3, "22:00", "02:00")));
}
