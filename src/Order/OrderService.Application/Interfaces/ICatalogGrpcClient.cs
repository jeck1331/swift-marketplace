using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface ICatalogGrpcClient
{
    Task<StockCheckResult> CheckStockAsync(Guid productId, int quantity, CancellationToken ct = default);
    Task<bool> ReserveStockAsync(Guid productId, int quantity, Guid orderId, CancellationToken ct = default);
    Task ReleaseStockAsync(Guid productId, int quantity, Guid orderId, CancellationToken ct = default);
}