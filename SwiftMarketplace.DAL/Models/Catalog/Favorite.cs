using SwiftMarketplace.DAL.Models.Identity;

namespace SwiftMarketplace.DAL.Models.Catalog;

public class Favorite
{
    public Guid ProductId { get; set; }
    // public Product Product { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
}