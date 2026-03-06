using FluentMigrator;

namespace CatalogService.Migrations.Migrations;

[Migration(2026030603)]
public sealed class EnablePgCrypto: Migration
{
    public override void Up() => Execute.Sql(@"CREATE EXTENSION IF NOT EXISTS pgcrypto;");

    public override void Down() { }
}