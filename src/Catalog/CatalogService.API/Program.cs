using CatalogService.API.Grpc;
using CatalogService.Application.Interfaces;
using Dapper;
using FluentMigrator.Runner;
using CatalogService.Infrastructure.Migrations;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Infrastructure.Search;
using Microsoft.OpenApi;
using Shared.Auth;

var builder = WebApplication.CreateBuilder(args);

DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddJwtAuth(builder.Configuration);

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
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });
    
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
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

app.UseAuthentication();
app.UseAuthorization();

// gRPC endpoint
app.MapGrpcService<CatalogGrpcService>();

app.MapControllers();

app.Run();