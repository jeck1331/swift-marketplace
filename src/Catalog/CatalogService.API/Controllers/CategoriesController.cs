using CatalogService.Application.DTOs.Category;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepo;

    public CategoriesController(ICategoryRepository categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken ct)
    {
        var categories = await _categoryRepo.GetAllAsync(ct);
        var response = categories.Select(c =>
            new CategoryResponse(c.Id, c.Name, c.Description, c.ParentId));
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> GetById(Guid id, CancellationToken ct)
    {
        var category = await _categoryRepo.GetByIdAsync(id, ct);
        if (category is null) return NotFound();

        return Ok(new CategoryResponse(category.Id, category.Name,
            category.Description, category.ParentId));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(
        [FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ParentId = request.ParentId,
            CreatedAt = DateTime.UtcNow
        };

        await _categoryRepo.CreateAsync(category, ct);

        return CreatedAtAction(nameof(GetById), new { id = category.Id },
            new CategoryResponse(category.Id, category.Name,
                category.Description, category.ParentId));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var existing = await _categoryRepo.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        existing.Name = request.Name;
        existing.Description = request.Description;

        await _categoryRepo.UpdateAsync(existing, ct);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _categoryRepo.DeleteAsync(id, ct);
        return NoContent();
    }
}