using Microsoft.EntityFrameworkCore;
using SwiftMarketplace.DAL.Models.Identity;
using SwiftMarketplace.DAL.Models.Ordering;
using SwiftMarketplace.DAL.Models.Catalog;

namespace SwiftMarketplace.DAL;

public class MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : DbContext(options)
{
    //Identity
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    //Catalog
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Product> Products { get; set; }

    //Ordering
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users", schema: "Identity");
        modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens", schema: "Identity");

        modelBuilder.Entity<Favorite>().ToTable("Favorites", schema: "Catalog");
        modelBuilder.Entity<Product>().ToTable("Products", schema: "Catalog");

        modelBuilder.Entity<Order>().ToTable("Orders", schema: "Ordering");
        modelBuilder.Entity<OrderItem>().ToTable("OrderItems", schema: "Ordering");

        modelBuilder.Entity<User>().HasKey(x => x.Id);
        modelBuilder.Entity<RefreshToken>().HasKey(x => x.Id);
        modelBuilder.Entity<Product>().HasKey(x => x.Id);
        modelBuilder.Entity<Order>().HasKey(x => x.Id);
        modelBuilder.Entity<OrderItem>().HasKey(x => x.Id);
        modelBuilder.Entity<Favorite>().HasKey(x => new { x.UserId, x.ProductId });

        modelBuilder.Entity<RefreshToken>()
            .HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .IsRequired();

        modelBuilder.Entity<OrderItem>()
            .HasOne(x => x.Order)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.OrderId)
            .IsRequired();
        modelBuilder.Entity<OrderItem>()
            .HasOne(x => x.Product)
            .WithOne(x => x.OrderItem)
            .HasForeignKey<OrderItem>(x => x.ProductId)
            .IsRequired();

        modelBuilder.Entity<Favorite>()
            .HasOne(x => x.User)
            .WithMany(x => x.Favorites)
            .HasForeignKey(x => x.UserId)
            .IsRequired();
        //#TODO Maybe many-to-many for relationships favorites and products
    }
}