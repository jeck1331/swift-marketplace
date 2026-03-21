using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;
[Migration(002)]
public class CreateProductTable: Migration {
    public override void Up()
    {
        Create.Table("products")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(300).NotNullable()
            .WithColumn("description").AsString(5000).Nullable()
            .WithColumn("price").AsDecimal(18, 2).NotNullable()
            .WithColumn("category_id").AsGuid().NotNullable()
                .ForeignKey("fk_products_category", "categories", "id")
            .WithColumn("image_url").AsString(1000).Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at").AsDateTimeOffset().Nullable();

        Create.Index("ix_products_category_id")
            .OnTable("products")
            .OnColumn("category_id");
        
        Create.Index("ix_products_is_active")
            .OnTable("products")
            .OnColumn("is_active");

        Create.Index("ix_productsprice")
            .OnTable("products")
            .OnColumn("price");
    }

    public override void Down() => Delete.Table("products");
}