using FluentMigrator;

namespace OrderService.Infrastructure.Migrations;

[Migration(002)]
public class CreateOrderItemsTable: Migration {
    public override void Up()
    {
        Create.Table("order_items")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("order_id").AsGuid().NotNullable()
            .ForeignKey("fk_order_items_order", "orders", "id")
            .WithColumn("product_id").AsGuid().NotNullable()
            .WithColumn("product_name").AsString(300).NotNullable()
            .WithColumn("price").AsDecimal(18, 2).NotNullable()
            .WithColumn("quantity").AsInt32().NotNullable();

        Create.Index("ix_order_items_order_id")
            .OnTable("order_items").OnColumn("order_id");
    }

    public override void Down() => Delete.Table("order_items");
}