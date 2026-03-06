using FluentMigrator;

namespace CatalogService.Migrations.Migrations;

[Migration(2026030604)]
public sealed class SeedProducts10k: Migration {
    public override void Up()
    {
        Execute.Sql(@"
    DO $$
    BEGIN
        IF (SELECT COUNT(*) FROM products) < 10000 THEN
            INSERT INTO products(id, name, created_at, price)
            SELECT
                gen_random_uuid(),
                'Product #' || gs::text,
                now() - (gs || ' seconds')::interval,
                (100 + (gs % 10000))::int
            FROM generate_series(1, 10000) gs;
        END IF
    END$$;
");
    }

    public override void Down()
    {
        Execute.Sql("TRUNCATE TABLE products");
    }
}