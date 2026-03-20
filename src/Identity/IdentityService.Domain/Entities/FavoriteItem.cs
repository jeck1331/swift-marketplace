namespace IdentityService.Domain.Entities;

public class FavoriteItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime AddedAt { get; set; }
}