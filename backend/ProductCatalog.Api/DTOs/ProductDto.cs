namespace ProductCatalog.Api.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int Quantity,
    int CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateProductDto(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int Quantity,
    int CategoryId
);

public record UpdateProductDto(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int Quantity,
    int CategoryId
);