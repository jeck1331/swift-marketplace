using FluentMigrator;

namespace CatalogService.Migrations.Migrations;

[Migration(2026030602)]
public sealed class AddPriceToProducts: Migration {
    public override void Up()
    {
        Alter.Table("products")
            .AddColumn("price").AsDecimal().NotNullable().WithDefaultValue("0");
            
    }

    public override void Down()
    {
        Delete.Column("price").FromTable("products");
    }
}