using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Models;
using ProductCatalog.Api.Repositories;

namespace ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return Ok(categories.Select(MapToDto));
    }

    /// <summary>
    /// GET /api/categories/tree — recursive hierarchical tree.
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult<IEnumerable<CategoryTreeDto>>> GetCategoryTree()
    {
        var all = (await _categoryRepository.GetAllAsync()).ToList();

        var childrenLookup = all
            .Where(c => c.ParentCategoryId.HasValue)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var roots = all
            .Where(c => c.ParentCategoryId is null)
            .Select(c => BuildTreeNode(c, childrenLookup))
            .OrderBy(c => c.Name)
            .ToList();

        return Ok(roots);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category switch
        {
            null => NotFound(new ErrorResponse($"Category with Id {id} not found.", 404)),
            _ => Ok(MapToDto(category))
        };
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var error = dto switch
        {
            { Name: null or "" } => "Name is required.",
            { Name.Length: > 100 } => "Name cannot exceed 100 characters.",
            _ => (string?)null
        };

        if (error is not null)
            return BadRequest(new ErrorResponse(error, 400));

        if (dto.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value);
            if (parent is null)
                return BadRequest(new ErrorResponse(
                    $"Parent category with Id {dto.ParentCategoryId.Value} does not exist.", 400));
        }

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            ParentCategoryId = dto.ParentCategoryId
        };

        var created = await _categoryRepository.AddAsync(category);

        return CreatedAtAction(nameof(GetCategory), new { id = created.Id }, MapToDto(created));
    }

    private static CategoryTreeDto BuildTreeNode(
        Category category, Dictionary<int, List<Category>> childrenLookup)
    {
        var children = childrenLookup.TryGetValue(category.Id, out var childList)
            ? childList.Select(c => BuildTreeNode(c, childrenLookup))
                       .OrderBy(c => c.Name).ToList()
            : new List<CategoryTreeDto>();

        return new CategoryTreeDto(
            Id: category.Id, Name: category.Name, Description: category.Description,
            ParentCategoryId: category.ParentCategoryId, Children: children
        );
    }

    private static CategoryDto MapToDto(Category c) => new(
        Id: c.Id, Name: c.Name, Description: c.Description,
        ParentCategoryId: c.ParentCategoryId
    );
}