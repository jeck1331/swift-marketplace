namespace OrderService.Application.DTOs;

public class StockCheckResult
{
    public bool IsAvailable { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal Price { get; set; }
    public string ProductName { get; set; } = null!;
}