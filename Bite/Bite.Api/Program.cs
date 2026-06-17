using Bite.Api.Middleware;
using Bite.Api.Webhooks;
using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Services.Implementation;
using Bite.Services.Interface;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    //options.ReturnHttpNotAcceptable = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
})
.AddXmlDataContractSerializerFormatters();

builder.Services.AddOpenApiDocument(settings =>
{
    settings.Title = "Bite API";
});

builder.Services.AddCors();

// Middleware for global exception handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ConnectionFactory
builder.Services.AddSingleton<IConnectionFactory>(_ =>
    DefaultConnectionFactory.FromConfiguration(
        builder.Configuration,
        "BiteDbConnection",
        "ProviderName"));

// Services
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderCodeService, OrderCodeService>();
builder.Services.AddScoped<IOrderStatusTokenService, OrderStatusTokenService>();
builder.Services.AddSingleton(TimeProvider.System);

// Webhook services
builder.Services.AddSingleton<IWebhookSender, WebhookSender>();
builder.Services.AddScoped<IOrderWebhookService, OrderWebhookService>();
builder.Services.AddHostedService<WebhookOutboxWorker>();

// DAOs
builder.Services.AddScoped<IRestaurantDao, RestaurantDao>();
builder.Services.AddScoped<IAddressDao, AddressDao>();
builder.Services.AddScoped<IOpeningHourSlotDao, OpeningHourSlotDao>();
builder.Services.AddScoped<IDeliveryZoneDao, DeliveryZoneDao>();
builder.Services.AddScoped<IDeliveryFeeRuleDao, DeliveryFeeRuleDao>();
builder.Services.AddScoped<ICustomerOrderDao, CustomerOrderDao>();
builder.Services.AddScoped<IOrderItemDao, OrderItemDao>();
builder.Services.AddScoped<IOrderStatusTokenDao, OrderStatusTokenDao>();
builder.Services.AddScoped<IMenuCategoryDao, MenuCategoryDao>();
builder.Services.AddScoped<IMenuItemDao, MenuItemDao>();
builder.Services.AddScoped<IWebhookOutboxDao, WebhookOutboxDao>();

var app = builder.Build();

// First in the pipeline so it wraps everything (controllers, filters, model binding).
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(settings =>
    {
        settings.Path = "/swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.Run();