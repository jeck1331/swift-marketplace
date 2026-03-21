using CatalogService.Domain.Entities;

namespace CatalogService.Application.DTOs;

public class SearchResult
{
    public IReadOnlyList<Product> Products { get; set; } = [];
    public long TotalCount { get; set; }
}