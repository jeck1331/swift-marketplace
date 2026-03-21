namespace CatalogService.Application.DTOs.Search;

public record SearchRequest(string? Query,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20);