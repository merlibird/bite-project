namespace Bite.Services.Common;

public static class GeoUtils
{
    public static double CalculateDistanceInKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        const double earthRadiusInKm = 6371;

        var latitudeDistance = ToRadians(latitude2 - latitude1);
        var longitudeDistance = ToRadians(longitude2 - longitude1);
        var currentLatitude = ToRadians(latitude1);
        var restaurantLatitude = ToRadians(latitude2);

        var a = Math.Sin(latitudeDistance / 2) * Math.Sin(latitudeDistance / 2) +
                Math.Cos(currentLatitude) * Math.Cos(restaurantLatitude) *
                Math.Sin(longitudeDistance / 2) * Math.Sin(longitudeDistance / 2);

        var c = 2 * Math.Asin(Math.Sqrt(a));

        return earthRadiusInKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
