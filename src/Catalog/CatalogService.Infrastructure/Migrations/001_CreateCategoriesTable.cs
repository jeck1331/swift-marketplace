using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(001)]
public class CreateCategoriesTable: Migration {
    public override void Up()
    {
        Create.Table("categories")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("parent_id").AsGuid().Nullable()
            .ForeignKey("fk_categories_parent", "categories", "id")
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();
    }

    public override void Down() => Delete.Table("categories");
}