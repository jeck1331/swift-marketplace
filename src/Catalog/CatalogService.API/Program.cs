using CatalogService.API.Grpc;
using CatalogService.Application.Interfaces;
using Dapper;
using FluentMigrator.Runner;
using CatalogService.Infrastructure.Migrations;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Infrastructure.Search;

var builder = WebApplication.CreateBuilder(args);

// ---- Dapper ----
DefaultTypeMap.MatchNamesWithUnderscores = true;

// ---- Repositories ----
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();

// ---- ElasticSearch ----
builder.Services.AddSingleton<IProductSearchService, ElasticProductSearchService>();

// ---- FluentMigrator ----
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(
            builder.Configuration.GetConnectionString("Postgres"))
        .ScanIn(typeof(CreateCategoriesTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

// ---- gRPC ----
builder.Services.AddGrpc();

// ---- REST ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Catalog Service API", Version = "v1" });
});

var app = builder.Build();

// ---- Migrations ----
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

app.UseSwagger();
app.UseSwaggerUI();

// gRPC endpoint
app.MapGrpcService<CatalogGrpcService>();

app.MapControllers();

app.Run();