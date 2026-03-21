using CatalogService.Application.DTOs;
using CatalogService.Domain.Entities;

namespace CatalogService.Application.Interfaces;

public interface IProductSearchService
{
    Task IndexProductAsync(Product product, CancellationToken ct = default);
    Task DeleteProductAsync(Guid productId, CancellationToken ct = default);
    
    Task<SearchResult> SearchAsync(
        string? query,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken ct = default);
}