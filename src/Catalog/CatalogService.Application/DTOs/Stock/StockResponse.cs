namespace CatalogService.Application.DTOs.Stock;

public record StockResponse(Guid ProductId, int Quantity, int Reserved, int Available);