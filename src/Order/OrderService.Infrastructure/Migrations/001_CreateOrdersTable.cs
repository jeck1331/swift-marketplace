using FluentMigrator;

namespace OrderService.Infrastructure.Migrations;

[Migration(001)]
public class CreateOrdersTable: Migration {
    public override void Up()
    {
        Create.Table("orders")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("total_amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at").AsDateTimeOffset().Nullable()
            .WithColumn("paid_at").AsDateTimeOffset().Nullable()
            .WithColumn("cancelled_at").AsDateTimeOffset().Nullable()
            .WithColumn("cancel_reason").AsString(500).Nullable();

        Create.Index("ix_orders_user_id")
            .OnTable("orders").OnColumn("user_id");

        Create.Index("ix_orders_status")
            .OnTable("orders").OnColumn("status");
    }

    public override void Down() => Delete.Table("orders");
}