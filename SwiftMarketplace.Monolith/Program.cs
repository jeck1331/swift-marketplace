using Microsoft.EntityFrameworkCore;
using SwiftMarketplace.DAL;

var builder = WebApplication.CreateBuilder(args);

//Register services
builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<MarketplaceDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("MarketplaceContext"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();