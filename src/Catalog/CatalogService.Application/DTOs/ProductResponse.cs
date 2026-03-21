namespace CatalogService.Application.DTOs;

public record ProductResponse(Guid Id,
    string Name,
    string? Description,
    decimal Price,
    Guid CategoryId,
    string? CategoryName,
    string? ImageUrl,
    bool IsActive,
    int AvailableStock);