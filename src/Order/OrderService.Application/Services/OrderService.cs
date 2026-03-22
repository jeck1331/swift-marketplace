using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using Shared.Contracts;
using Shared.Contracts.Events;

namespace OrderService.Application.Services;

public class OrderService
{
     private readonly IOrderRepository _orderRepo;
    private readonly IOrderItemRepository _orderItemRepo;
    private readonly ICatalogGrpcClient _catalogClient;
    private readonly IEventPublisher _eventPublisher;

    public OrderService(
        IOrderRepository orderRepo,
        IOrderItemRepository orderItemRepo,
        ICatalogGrpcClient catalogClient,
        IEventPublisher eventPublisher)
    {
        _orderRepo = orderRepo;
        _orderItemRepo = orderItemRepo;
        _catalogClient = catalogClient;
        _eventPublisher = eventPublisher;
    }

    public async Task<OrderResponse> CreateOrderAsync(
        Guid userId, CreateOrderRequest request, CancellationToken ct)
    {
        // 1. Проверяем наличие через gRPC
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var stockCheck = await _catalogClient.CheckStockAsync(
                item.ProductId, item.Quantity, ct);

            if (!stockCheck.IsAvailable)
                throw new InvalidOperationException(
                    $"Product {item.ProductId}: недостаточно товара. " +
                    $"Доступно: {stockCheck.AvailableQuantity}, запрошено: {item.Quantity}");

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                ProductName = stockCheck.ProductName,
                Price = stockCheck.Price,
                Quantity = item.Quantity
            });

            totalAmount += stockCheck.Price * item.Quantity;
        }

        // 2. Создаём заказ в БД
        var order = new Domain.Entities.Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow
        };

        await _orderRepo.CreateAsync(order, ct);

        foreach (var item in orderItems)
            item.OrderId = order.Id;

        await _orderItemRepo.CreateManyAsync(orderItems, ct);

        // 3. Резервируем сток через gRPC
        var reservedItems = new List<OrderItem>();
        try
        {
            foreach (var item in orderItems)
            {
                var reserved = await _catalogClient.ReserveStockAsync(
                    item.ProductId, item.Quantity, order.Id, ct);

                if (!reserved)
                {
                    // Откатываем уже зарезервированные
                    foreach (var ri in reservedItems)
                    {
                        await _catalogClient.ReleaseStockAsync(
                            ri.ProductId, ri.Quantity, order.Id, ct);
                    }

                    await _orderRepo.UpdateStatusAsync(order.Id, OrderStatus.Failed, ct);
                    throw new InvalidOperationException(
                        $"Не удалось зарезервировать товар {item.ProductName}");
                }

                reservedItems.Add(item);
            }
        }
        catch (Exception) when (reservedItems.Count > 0)
        {
            // Compensating: откатываем все резервы при любой ошибке
            foreach (var ri in reservedItems)
            {
                await _catalogClient.ReleaseStockAsync(
                    ri.ProductId, ri.Quantity, order.Id, ct);
            }
            throw;
        }

        // 4. Обновляем статус
        await _orderRepo.UpdateStatusAsync(order.Id, OrderStatus.StockReserved, ct);
        order.Status = OrderStatus.StockReserved;

        // 5. Публикуем событие в Kafka → Payment подхватит
        await _eventPublisher.PublishAsync(
            KafkaTopics.OrderCreated,
            order.Id.ToString(),
            new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserId = userId,
                TotalAmount = totalAmount,
                CreatedAt = order.CreatedAt,
                Items = orderItems.Select(i => new OrderItemEvent
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            }, ct);

        return MapToResponse(order, orderItems);
    }

    public async Task<OrderResponse> GetOrderAsync(
        Guid userId, Guid orderId, CancellationToken ct)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        if (order.UserId != userId)
            throw new UnauthorizedAccessException("Access denied");

        var items = await _orderItemRepo.GetByOrderIdAsync(orderId, ct);
        return MapToResponse(order, items);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetUserOrdersAsync(
        Guid userId, int page, int pageSize, CancellationToken ct)
    {
        var orders = await _orderRepo.GetByUserIdAsync(userId, page, pageSize, ct);
        var result = new List<OrderResponse>();

        foreach (var order in orders)
        {
            var items = await _orderItemRepo.GetByOrderIdAsync(order.Id, ct);
            result.Add(MapToResponse(order, items));
        }

        return result;
    }

    /// <summary>
    /// Вызывается Kafka consumer при PaymentSucceeded
    /// </summary>
    public async Task HandlePaymentSucceededAsync(
        Guid orderId, DateTime paidAt, CancellationToken ct)
    {
        await _orderRepo.SetPaidAsync(orderId, paidAt, ct);
    }

    /// <summary>
    /// Вызывается Kafka consumer при PaymentFailed
    /// </summary>
    public async Task HandlePaymentFailedAsync(
        Guid orderId, string reason, CancellationToken ct)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, ct);
        if (order is null) return;

        var items = await _orderItemRepo.GetByOrderIdAsync(orderId, ct);

        // Compensating: освобождаем резерв
        foreach (var item in items)
        {
            await _catalogClient.ReleaseStockAsync(
                item.ProductId, item.Quantity, orderId, ct);
        }

        await _orderRepo.SetCancelledAsync(orderId, reason, DateTime.UtcNow, ct);

        // Публикуем OrderCancelled
        await _eventPublisher.PublishAsync(
            KafkaTopics.OrderCancelled,
            orderId.ToString(),
            new OrderCancelledEvent
            {
                OrderId = orderId,
                UserId = order.UserId,
                Reason = reason,
                CancelledAt = DateTime.UtcNow,
                Items = items.Select(i => new OrderItemEvent
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            }, ct);
    }

    private static OrderResponse MapToResponse(
        Domain.Entities.Order order, IEnumerable<OrderItem> items)
    {
        return new OrderResponse(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.TotalAmount,
            items.Select(i => new OrderItemResponse(
                i.ProductId, i.ProductName, i.Price, i.Quantity,
                i.Price * i.Quantity)).ToList(),
            order.CreatedAt,
            order.PaidAt,
            order.CancelledAt,
            order.CancelReason);
    }
}