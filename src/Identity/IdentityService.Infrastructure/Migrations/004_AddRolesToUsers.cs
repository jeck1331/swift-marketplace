using FluentMigrator;

namespace IdentityService.Infrastructure.Migrations;

[Migration(004)]
public class AddRolesToUsers: Migration {
    public override void Up()
    {
        Alter.Table("users")
            .AddColumn("role").AsString(50).NotNullable().WithDefaultValue("User");
    }

    public override void Down()
    {
        Delete.Column("role").FromTable("users");
    }
}