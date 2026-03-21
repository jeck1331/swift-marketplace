using CatalogService.Domain.Entities;

namespace CatalogService.Application.Interfaces;

public interface IStockRepository
{
    Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task SetStockAsync(Guid productId, int quantity, CancellationToken ct = default);
    
    /// <summary>
    /// Атомарное резервирование. Возвращает true если хватило остатков.
    /// </summary>
    Task<bool> ReserveAsync(Guid productId, int quantity, CancellationToken ct = default);
    
    /// <summary>
    /// Отмена резерва (compensating transaction).
    /// </summary>
    Task ReleaseReserveAsync(Guid productId, int quantity, CancellationToken ct = default);
    
    /// <summary>
    /// Подтверждение списания (после оплаты).
    /// </summary>
    Task ConfirmReserveAsync(Guid productId, int quantity, CancellationToken ct = default);
}