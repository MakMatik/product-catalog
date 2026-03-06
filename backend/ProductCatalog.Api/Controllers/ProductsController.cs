using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Models;
using ProductCatalog.Api.Repositories;
using ProductCatalog.Api.Serialization;
using ProductCatalog.Api.Services;

namespace ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ProductSearchEngine _searchEngine;

    public ProductsController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ProductSearchEngine searchEngine)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _searchEngine = searchEngine;
    }

    /// <summary>
    /// GET /api/products — paginated, filterable, searchable.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ProductDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null)
    {
        // Pattern matching validation
        var validationError = (page, pageSize) switch
        {
            ( < 1, _) => "Page must be at least 1.",
            (_, < 1) => "PageSize must be at least 1.",
            (_, > 100) => "PageSize cannot exceed 100.",
            _ => (string?)null
        };

        if (validationError is not null)
            return BadRequest(new ErrorResponse(validationError, 400));

        var (products, totalCount) = await _productRepository.GetPaginatedAsync(
            page, pageSize, categoryId, search, sortBy);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedResult<ProductDto>(
            Items: products.Select(MapToDto),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        ));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product switch
        {
            null => NotFound(new ErrorResponse($"Product with Id {id} not found.", 404)),
            _ => Ok(MapToDto(product))
        };
    }

    /// <summary>
    /// POST /api/products — pattern matching validation.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        var error = dto switch
        {
            { Name: null or "" } => "Name is required.",
            { Name.Length: > 200 } => "Name cannot exceed 200 characters.",
            { SKU: null or "" } => "SKU is required.",
            { Price: < 0 } => "Price cannot be negative.",
            { Quantity: < 0 } => "Quantity cannot be negative.",
            { CategoryId: < 1 } => "Valid CategoryId is required.",
            _ => (string?)null
        };

        if (error is not null)
            return BadRequest(new ErrorResponse(error, 400));

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category is null)
            return BadRequest(new ErrorResponse($"Category with Id {dto.CategoryId} does not exist.", 400));

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Price = dto.Price,
            Quantity = dto.Quantity,
            CategoryId = dto.CategoryId
        };

        var created = await _productRepository.AddAsync(product);
        await RefreshSearchIndex();

        return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, MapToDto(created));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new ErrorResponse($"Product with Id {id} not found.", 404));

        var error = dto switch
        {
            { Name: null or "" } => "Name is required.",
            { Name.Length: > 200 } => "Name cannot exceed 200 characters.",
            { SKU: null or "" } => "SKU is required.",
            { Price: < 0 } => "Price cannot be negative.",
            { Quantity: < 0 } => "Quantity cannot be negative.",
            _ => (string?)null
        };

        if (error is not null)
            return BadRequest(new ErrorResponse(error, 400));

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category is null)
            return BadRequest(new ErrorResponse($"Category with Id {dto.CategoryId} does not exist.", 400));

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.SKU = dto.SKU;
        existing.Price = dto.Price;
        existing.Quantity = dto.Quantity;
        existing.CategoryId = dto.CategoryId;

        var updated = await _productRepository.UpdateAsync(existing);
        await RefreshSearchIndex();

        return Ok(MapToDto(updated));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var exists = await _productRepository.GetByIdAsync(id);
        if (exists is null)
            return NotFound(new ErrorResponse($"Product with Id {id} not found.", 404));

        await _productRepository.DeleteAsync(id);
        await RefreshSearchIndex();

        return NoContent();
    }

    /// <summary>
    /// GET /api/products/search?q=lptop — fuzzy search with custom JSON serialization.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string q, [FromQuery] int maxResults = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new ErrorResponse("Search query 'q' is required.", 400));

        await RefreshSearchIndex();

        var results = _searchEngine.Search(q, maxResults);

        var searchResults = results.Select(r => new { score = r.Score, product = r.Item });

        // Custom JSON serialization using ProductSearchResultConverter
        var json = JsonSerializer.Serialize(searchResults, JsonSerializerConfig.WithSearchConverter);
        return Content(json, "application/json");
    }

    /// <summary>
    /// POST /api/products/manual-bind — demonstrates manual model binding
    /// by reading raw body instead of using [FromBody].
    /// </summary>
    [HttpPost("manual-bind")]
    public async Task<ActionResult<ProductDto>> CreateProductManualBind()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
            return BadRequest(new ErrorResponse("Request body is empty.", 400));

        CreateProductDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<CreateProductDto>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            return BadRequest(new ErrorResponse($"Invalid JSON: {ex.Message}", 400));
        }

        if (dto is null)
            return BadRequest(new ErrorResponse("Could not parse product data.", 400));

        return await CreateProduct(dto);
    }

    private async Task RefreshSearchIndex()
    {
        var allProducts = await _productRepository.GetAllAsync();
        _searchEngine.UpdateIndex(allProducts);
    }

    private static ProductDto MapToDto(Product p) => new(
        Id: p.Id, Name: p.Name, Description: p.Description, SKU: p.SKU,
        Price: p.Price, Quantity: p.Quantity, CategoryId: p.CategoryId,
        CategoryName: p.Category?.Name, CreatedAt: p.CreatedAt, UpdatedAt: p.UpdatedAt
    );
}