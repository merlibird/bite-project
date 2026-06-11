using Bite.Domain;

namespace Bite.Api.Dtos.Mappers;

public static class RestaurantMapper
{
    public static RestaurantSearchResultDto ToRestaurantSearchResultDto(
        this IReadOnlyCollection<(
            Restaurant Restaurant,
            Address Address,
            double DistanceInKm,
            bool IsOpenNow,
            IReadOnlyCollection<OpeningHourSlot> OpeningHours)> restaurants,
        double latitude,
        double longitude,
        bool openNowOnly,
        int count)
    {
        return new RestaurantSearchResultDto
        {
            Latitude = latitude,
            Longitude = longitude,
            OpenNowOnly = openNowOnly,
            Count = count,
            Restaurants = restaurants
                .Select(item => new RestaurantSearchItemDto
                {
                    Id = item.Restaurant.Id,
                    Name = item.Restaurant.Name,
                    TitleImagePath = item.Restaurant.TitleImagePath,
                    Address = new RestaurantAddressDto
                    {
                        Street = item.Address.Street,
                        Number = item.Address.Number,
                        ZipCode = item.Address.ZipCode,
                        City = item.Address.City,
                        Country = item.Address.Country,
                        Latitude = item.Address.Latitude,
                        Longitude = item.Address.Longitude,
                        AdditionalInfo = item.Address.AdditionalInfo
                    },
                    DistanceInKm = item.DistanceInKm,
                    IsOpenNow = item.IsOpenNow,
                    OpeningHours = item.OpeningHours
                        .Select(slot => new OpeningHourSlotDto
                        {
                            DayOfWeek = slot.DayOfWeek,
                            OpenTime = slot.OpenTime,
                            CloseTime = slot.CloseTime
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
