namespace OrderService.Application.DTOs;

public record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity,
    decimal Subtotal);