namespace CatalogService.Application.DTOs;

public record CreateProductRequest(string Name,
    string? Description,
    decimal Price,
    Guid CategoryId,
    string? ImageUrl,
    int InitialStock);