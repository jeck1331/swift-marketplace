namespace CatalogService.Domain.Entities;

public class StockItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int Reserved { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int Available => Quantity - Reserved;
}