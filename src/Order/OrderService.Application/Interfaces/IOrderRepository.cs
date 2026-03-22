using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<Guid> CreateAsync(Order order, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct = default);
    Task SetPaidAsync(Guid id, DateTime paidAt, CancellationToken ct = default);
    Task SetCancelledAsync(Guid id, string reason, DateTime cancelledAt, CancellationToken ct = default);
}