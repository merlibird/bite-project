using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Interface;
using System.Transactions;
using Microsoft.AspNetCore.Hosting;

namespace Bite.Services.Implementation;

public class RestaurantService(
    IRestaurantDao restaurantDao,
    IAddressDao addressDao,
    IOpeningHourSlotDao openingHourSlotDao,
    IApiKeyService apiKeyService) : IRestaurantService
{
    //private const string ImageBaseDir = "wwwroot/images/restaurants";

    public async Task<ServiceResult<(int RestaurantId, string RawApiKey)>> RegisterAsync(
        Restaurant restaurant,
        Address address,
        List<OpeningHourSlot> openingHours,
        Stream? imageStream,
        string? imageExtension,
        string webRootPath,
        CancellationToken cancellationToken = default)
    {
        var existing = await restaurantDao.FindByNameAndCityAsync(restaurant.Name, address.City, cancellationToken);
        if (existing != null)
        {
            return ServiceResult<(int, string)>.Failure(
                $"Restaurant '{restaurant.Name}' in city '{address.City}' already exists.", 
                ServiceResultType.Conflict);
        }

        string? imagePath = null;
        if (imageStream != null && !string.IsNullOrEmpty(imageExtension))
        {
            if (imageStream.Length < 10)
            {
                return ServiceResult<(int, string)>.Failure(
                    $"The uploaded image is too small or invalid ({imageStream.Length} bytes received).",
                    ServiceResultType.Error);
            }

            var fileName = $"{Guid.NewGuid()}{imageExtension}";
            var fullPath = Path.Combine(webRootPath, "images", "restaurants", fileName);

            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var fileStream = new FileStream(fullPath, FileMode.Create);
            await imageStream.CopyToAsync(fileStream, cancellationToken);
            imagePath = $"/images/restaurants/{fileName}";
        }

        string rawApiKey = apiKeyService.GenerateApiKey();
        string hashedApiKey = apiKeyService.HashApiKey(rawApiKey);

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // 1. Insert Address
        int addressId = await addressDao.InsertAsync(address, cancellationToken);

        // 2. Insert Restaurant
        var restaurantToInsert = new Restaurant(
            id: 0,
            name: restaurant.Name,
            addressId: addressId,
            webhookUrl: restaurant.WebhookUrl,
            apiKey: hashedApiKey,
            titleImagePath: imagePath
        );

        int restaurantId = await restaurantDao.InsertAsync(restaurantToInsert, cancellationToken);

        // 3. Insert Opening Hours
        foreach (var slot in openingHours)
        {
            var slotToInsert = new OpeningHourSlot(
                id: 0,
                restaurantId: restaurantId,
                dayOfWeek: slot.DayOfWeek,
                openTime: slot.OpenTime,
                closeTime: slot.CloseTime
            );
            await openingHourSlotDao.InsertAsync(slotToInsert, cancellationToken);
        }

        scope.Complete();

        return ServiceResult<(int, string)>.Success((restaurantId, rawApiKey));
    }
}
