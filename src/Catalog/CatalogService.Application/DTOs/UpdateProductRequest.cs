namespace CatalogService.Application.DTOs;

public record UpdateProductRequest(string Name,
    string? Description,
    decimal Price,
    Guid CategoryId,
    string? ImageUrl);