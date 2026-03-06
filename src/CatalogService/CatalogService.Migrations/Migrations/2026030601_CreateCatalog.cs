using FluentMigrator;

namespace CatalogService.Migrations.Migrations;

[Migration(2026030601)]
public sealed class CreateCatalog : Migration {
    public override void Up()
    {
        Create.Table("products")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(250).NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();

        Create.Index("IX_products_created_at_id")
            .OnTable("products")
            .OnColumn("created_at").Descending()
            .OnColumn("id").Ascending();
        
        Insert.IntoTable("products").Row(new
            { 
                id = System.Guid.Parse("11111111-1111-1111-1111-111111111111"),
                name = "Test Product 1",
                created_at = System.DateTimeOffset.UtcNow 
            }
        );
    }

    public override void Down()
    {
        Delete.Table("products");
    }
}