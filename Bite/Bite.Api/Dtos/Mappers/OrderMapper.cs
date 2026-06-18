using Bite.Domain;

namespace Bite.Api.Dtos.Mappers;

public static class OrderMapper
{
    public static Address ToDomain(this RestaurantAddressDto dto)
    {
        return new Address(
            id: 0,
            street: dto.Street,
            number: dto.Number,
            zipCode: dto.ZipCode,
            city: dto.City,
            country: dto.Country,
            longitude: dto.Longitude,
            latitude: dto.Latitude,
            additionalInfo: dto.AdditionalInfo
        );
    }

    public static IEnumerable<(int MenuItemId, int Quantity)> ToItemTuples(this IEnumerable<OrderItemDto> items)
        => items.Select(item => (item.MenuItemId, item.Quantity));
}
