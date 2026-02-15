using SwiftMarketplace.DAL.Models.Catalog;

namespace SwiftMarketplace.DAL.Models.Ordering;

public class OrderItem
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public decimal PriceSnapshot { get; set; }
    public string ProductNameSnapshot { get; set; }
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
}