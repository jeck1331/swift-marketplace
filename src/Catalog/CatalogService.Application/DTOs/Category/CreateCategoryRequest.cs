namespace CatalogService.Application.DTOs.Category;

public record CreateCategoryRequest(string Name, string? Description, Guid? ParentId);