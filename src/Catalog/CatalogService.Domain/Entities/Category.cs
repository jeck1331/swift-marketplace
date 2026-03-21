namespace CatalogService.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public DateTime CreatedAt { get; set; }
}