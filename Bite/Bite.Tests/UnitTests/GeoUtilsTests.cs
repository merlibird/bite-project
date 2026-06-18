using Bite.Services.Common;

namespace Bite.Tests.UnitTests;

public class GeoUtilsTests
{
    [Fact]
    public void CalculateDistanceInKm_SameCoordinates_ReturnsZero()
    {
        var d = GeoUtils.CalculateDistanceInKm(48.2, 16.37, 48.2, 16.37);
        Assert.Equal(0, d, precision: 6);
    }

    [Fact]
    public void CalculateDistanceInKm_ViennaToSalzburg_ReturnsAboutTwoHundredFiftyKm()
    {
        // Vienna (48.2082, 16.3738) -> Salzburg (47.8095, 13.0550): ~250 km
        var d = GeoUtils.CalculateDistanceInKm(48.2082, 16.3738, 47.8095, 13.0550);
        Assert.InRange(d, 245, 255);
    }

    [Fact]
    public void CalculateDistanceInKm_SameCoordinatesReversed_ReturnsSameDistance()
    {
        var ab = GeoUtils.CalculateDistanceInKm(48.2, 16.37, 47.8, 13.05);
        var ba = GeoUtils.CalculateDistanceInKm(47.8, 13.05, 48.2, 16.37);
        Assert.Equal(ab, ba, precision: 9);
    }
}
