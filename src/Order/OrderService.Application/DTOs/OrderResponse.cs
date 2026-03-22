namespace OrderService.Application.DTOs;

public record OrderResponse(
    Guid Id,
    Guid UserId,
    string Status,
    decimal TotalAmount,
    List<OrderItemResponse> Items,
    DateTime CreatedAt,
    DateTime? PaidAt,
    DateTime? CancelledAt,
    string? CancelReason);