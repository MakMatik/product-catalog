using System.Text.Json;
using System.Text.Json.Serialization;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Serialization;

/// <summary>
/// Custom JSON converter that adds computed fields (formattedPrice, stockStatus)
/// during serialization without modifying the domain model.
/// </summary>
public class ProductSearchResultConverter : JsonConverter<Product>
{
    public override Product? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Deserialization not supported by this converter.");

    public override void Write(Utf8JsonWriter writer, Product product, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteNumber("id", product.Id);
        writer.WriteString("name", product.Name);
        writer.WriteString("description", product.Description);
        writer.WriteString("sku", product.SKU);
        writer.WriteString("formattedPrice", $"${product.Price:N2}");
        writer.WriteNumber("price", product.Price);

        // Pattern matching for stock status
        writer.WriteString("stockStatus", product switch
        {
            { Quantity: 0 } => "OUT_OF_STOCK",
            { Quantity: > 0 and <= 10 } => "LOW_STOCK",
            _ => "IN_STOCK"
        });
        writer.WriteNumber("quantity", product.Quantity);

        writer.WriteNumber("categoryId", product.CategoryId);
        writer.WriteString("categoryName", product.Category?.Name ?? "Unknown");
        writer.WriteString("createdAt", product.CreatedAt.ToString("O"));
        writer.WriteString("updatedAt", product.UpdatedAt.ToString("O"));

        writer.WriteEndObject();
    }
}

public static class JsonSerializerConfig
{
    public static JsonSerializerOptions Default => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static JsonSerializerOptions WithSearchConverter => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new ProductSearchResultConverter() }
    };
}