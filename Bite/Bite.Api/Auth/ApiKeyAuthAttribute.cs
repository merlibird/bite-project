using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bite.Api.Auth;

public sealed class ApiKeyAuthAttribute : Attribute, IAsyncAuthorizationFilter
{
    public const string HeaderName = "X-Api-Key";
    public const string RestaurantIdItem = "AuthenticatedRestaurantId";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;
        string apiKey = http.Request.Headers[HeaderName].ToString();
        Restaurant? restaurant = null;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var apiKeyService = http.RequestServices.GetRequiredService<IApiKeyService>();
            var restaurantDao = http.RequestServices.GetRequiredService<IRestaurantDao>();
            restaurant = await restaurantDao.FindByApiKeyAsync(apiKeyService.HashApiKey(apiKey), http.RequestAborted);
        }

        if (restaurant is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid or missing API key." });
            return;
        }

        http.Items[RestaurantIdItem] = restaurant.Id;
    }
}
