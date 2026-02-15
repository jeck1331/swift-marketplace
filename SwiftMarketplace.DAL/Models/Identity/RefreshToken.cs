namespace SwiftMarketplace.DAL.Models.Identity;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }
}