using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;
[Migration(003)]
public class CreateStockTable: Migration {
    public override void Up()
    {
        Create.Table("stock")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("product_id").AsGuid().NotNullable().Unique()
            .ForeignKey("fk_stock_product", "products", "id")
            .WithColumn("quantity").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("reserved").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable();
    }

    public override void Down() => Delete.Table("stock");
}