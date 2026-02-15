using SwiftMarketplace.DAL.Models.Ordering;

namespace SwiftMarketplace.DAL.Models.Catalog;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Attributes { get; set; }
    public string? ImagePath { get; set; }
    public bool isDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = new DateTime().ToUniversalTime();
    public DateTime UpdatedAt { get; set; }
    
    public Guid? OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }
    
    // public Guid? FavoriteId { get; set; }
    // public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}