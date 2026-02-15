using SwiftMarketplace.Common.Models.Enums;
using SwiftMarketplace.DAL.Models.Identity;

namespace SwiftMarketplace.DAL.Models.Ordering;

public class Order
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public string Address { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}