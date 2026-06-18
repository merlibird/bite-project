using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Implementation;
using Bite.Services.Interface;
using NSubstitute;

// This test class was created with the help and assistance of AI 
namespace Bite.Tests.UnitTests;

public class RestaurantServiceRegisterImageTests
{
    private readonly IRestaurantDao restaurantDao = Substitute.For<IRestaurantDao>();
    private readonly IAddressDao addressDao = Substitute.For<IAddressDao>();
    private readonly IOpeningHourSlotDao openingHourSlotDao = Substitute.For<IOpeningHourSlotDao>();
    private readonly IApiKeyService apiKeyService = Substitute.For<IApiKeyService>();
    private readonly IDeliveryZoneDao deliveryZoneDao = Substitute.For<IDeliveryZoneDao>();
    private readonly IDeliveryFeeRuleDao deliveryFeeRuleDao = Substitute.For<IDeliveryFeeRuleDao>();
    private readonly TimeProvider timeProvider = TimeProvider.System;

    public RestaurantServiceRegisterImageTests()
    {
        // No existing restaurant with the same name/city -> validation path is reached.
        restaurantDao.FindByNameAndCityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Restaurant?)null);
        apiKeyService.GenerateApiKey().Returns("raw-key");
        apiKeyService.HashApiKey(Arg.Any<string>()).Returns("hashed-key");
    }

    private RestaurantService CreateService() => new(
        restaurantDao, addressDao, openingHourSlotDao, apiKeyService,
        timeProvider, deliveryZoneDao, deliveryFeeRuleDao);

    private static readonly Restaurant Restaurant = new(0, "Testaurant", 0, "https://hook", "");
    private static readonly Address Address = new(0, "Street", "1", "1010", "Vienna", "AT", longitude: 0, latitude: 0);

    private Task<ServiceResult<(int RestaurantId, string RawApiKey)>> Register(
        Stream imageStream, string imageExtension, string webRootPath = "")
        => CreateService().RegisterAsync(Restaurant, Address, [], imageStream, imageExtension, webRootPath);

    private static MemoryStream PngStream()
        => new([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D]);

    private static MemoryStream JpegStream()
        => new([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01]);

    [Fact]
    public async Task RegisterAsync_UnsupportedExtension_ReturnsValidationError()
    {
        var result = await Register(PngStream(), ".gif");

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.ValidationError, result.ResultType);
        Assert.Contains("Unsupported image type", result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_OversizedImage_ReturnsValidationError()
    {
        using var oversized = new MemoryStream(new byte[6 * 1024 * 1024]);

        var result = await Register(oversized, ".png");

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.ValidationError, result.ResultType);
        Assert.Contains("maximum size", result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_UnrecognizedHeader_ReturnsValidationError()
    {
        using var garbage = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });

        var result = await Register(garbage, ".png");

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.ValidationError, result.ResultType);
        Assert.Contains("not a valid image", result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_ContentExtensionMismatch_ReturnsValidationError()
    {
        // Real PNG content but claimed as .jpg.
        var result = await Register(PngStream(), ".jpg");

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.ValidationError, result.ResultType);
        Assert.Contains("does not match the file extension", result.ErrorMessage);
    }

    [Theory]
    [InlineData(".png")]
    [InlineData(".jpg")]
    public async Task RegisterAsync_ValidImage_IsAccepted(string extension)
    {
        using var image = extension == ".png" ? PngStream() : JpegStream();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"bite-test-{Guid.NewGuid()}");

        try
        {
            var result = await Register(image, extension, tempRoot);

            Assert.True(result.IsSuccess);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}
