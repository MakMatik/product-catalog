namespace ProductCatalog.Api.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string Description,
    int? ParentCategoryId
);

public record CategoryTreeDto(
    int Id,
    string Name,
    string Description,
    int? ParentCategoryId,
    List<CategoryTreeDto> Children
);

public record CreateCategoryDto(
    string Name,
    string Description,
    int? ParentCategoryId
);