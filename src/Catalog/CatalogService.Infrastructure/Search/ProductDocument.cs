namespace CatalogService.Infrastructure.Search;

public class ProductDocument
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}