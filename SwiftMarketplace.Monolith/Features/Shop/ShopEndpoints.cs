namespace SwiftMarketplace.Monolith.Features.Shop;

public static class ShopEndpoints
{
    public static void MapShopEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/shop");
        
        group.MapGet("/", () => "Hello World!");
        group.MapPost("/", GetProduct);
    }
    
    static async Task<IResult> GetProduct(int id)
    {
        return Results.NotFound();
    }
}