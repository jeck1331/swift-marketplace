using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderItemRepository
{
    Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task CreateManyAsync(IEnumerable<OrderItem> items, CancellationToken ct = default);
}