using Bite.Domain;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bite.Api.Json;
public sealed class OrderStatusJsonConverter : JsonConverter<OrderStatus>
{
    public override OrderStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null || !OrderStatusExtensions.IsValidOrderStatus(value))
        {
            throw new JsonException($"Invalid order status '{value}'.");
        }

        return OrderStatusExtensions.FromDbValue(value);
    }

    public override void Write(Utf8JsonWriter writer, OrderStatus value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToDbValue());
}
