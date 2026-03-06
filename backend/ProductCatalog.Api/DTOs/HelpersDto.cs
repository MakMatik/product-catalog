namespace ProductCatalog.Api.DTOs;

public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record ProductSearchResult(
    ProductDto Product,
    double Score
);

public record ValidationErrorResponse(
    string Message,
    Dictionary<string, string[]> Errors
);

public record ErrorResponse(
    string Message,
    int StatusCode
);