using CatalogService.Domain.Entities;

namespace CatalogService.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId, int page, int pageSize, CancellationToken ct = default);
    
    Task<IReadOnlyList<Product>> GetAllAsync(
        int page, int pageSize, CancellationToken ct = default);
    
    Task<Guid> CreateAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    
    Task<int> GetTotalCountAsync(Guid? categoryId = null, CancellationToken ct = default);
}