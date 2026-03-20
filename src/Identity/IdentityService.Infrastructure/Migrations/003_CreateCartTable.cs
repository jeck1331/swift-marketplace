using FluentMigrator;

namespace IdentityService.Infrastructure.Migrations;

[Migration(003)]
public class CreateCartTable: Migration {
    public override void Up()
    {
        Create.Table("cart_items")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("product_id").AsGuid().NotNullable()
            .WithColumn("quantity").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("added_at").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at").AsDateTimeOffset().Nullable();

        Create.UniqueConstraint("uq_cart_user_product")
            .OnTable("cart_items")
            .Columns("user_id", "product_id");
    }

    public override void Down()
    {
        Delete.Table("cart_items");
    }
}