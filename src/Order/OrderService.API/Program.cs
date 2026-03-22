
using System.IdentityModel.Tokens.Jwt;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.OpenApi;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Grpc;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Migrations;
using OrderService.Infrastructure.Repositories;
using Shared.Auth;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
DefaultTypeMap.MatchNamesWithUnderscores = true;

// ── Auth ──
builder.Services.AddJwtAuth(builder.Configuration);

// ── Repositories ──
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();

// ── gRPC client ──
builder.Services.AddSingleton<ICatalogGrpcClient, CatalogGrpcClient>();

// ── Kafka ──
builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
builder.Services.AddHostedService<PaymentResultConsumer>();

// ── Services ──
builder.Services.AddScoped<OrderService.Application.Services.OrderService>();

// ── FluentMigrator ──
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(
            builder.Configuration.GetConnectionString("Postgres"))
        .ScanIn(typeof(CreateOrdersTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Order Service API", Version = "v1" });
    
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

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();