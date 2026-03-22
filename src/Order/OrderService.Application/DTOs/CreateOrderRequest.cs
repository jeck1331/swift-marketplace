namespace OrderService.Application.DTOs;

public record CreateOrderRequest(List<CreateOrderItemRequest> Items);