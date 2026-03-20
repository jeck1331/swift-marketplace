using FluentMigrator;

namespace IdentityService.Infrastructure.Migrations;

[Migration(001)]
public class CreateUserTable: Migration {
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("email").AsString(255).NotNullable().Unique()
            .WithColumn("password_hash").AsString(500).NotNullable()
            .WithColumn("first_name").AsString(100).NotNullable()
            .WithColumn("last_name").AsString(100).NotNullable()
            .WithColumn("phone").AsString(20).Nullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at").AsDateTimeOffset().Nullable();

        Create.Index("ix_users_email")
            .OnTable("users")
            .OnColumn("email");
    }

    public override void Down()
    {
        Delete.Table("users");
    }
}