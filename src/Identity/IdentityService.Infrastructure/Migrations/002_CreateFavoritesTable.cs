using FluentMigrator;

namespace IdentityService.Infrastructure.Migrations;

[Migration(002)]
public class CreateFavoritesTable: Migration {
    public override void Up()
    {
        Create.Table("favorites")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("product_id").AsGuid().NotNullable()
            .WithColumn("added_at").AsDateTimeOffset().NotNullable();

        Create.Index("ix_favorites_user_id")
            .OnTable("favorites")
            .OnColumn("user_id");

        Create.UniqueConstraint("uq_favorites_user_product")
            .OnTable("favorites")
            .Columns("user_id", "product_id");
    }

    public override void Down()
    {
        Delete.Table("favorites");
    }
}