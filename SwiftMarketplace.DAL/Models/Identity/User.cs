using SwiftMarketplace.Common.Models.Enums;
using SwiftMarketplace.DAL.Models.Catalog;
using SwiftMarketplace.DAL.Models.Ordering;

namespace SwiftMarketplace.DAL.Models.Identity;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}