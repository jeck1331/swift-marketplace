namespace SwiftMarketplace.Monolith.Features.Catalog;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/catalog");
        
        group.MapGet("/", () => "Hello World!");
        group.MapPost("/", GetProduct);
    }
    
    static async Task<IResult> GetProduct(int id)
    {
        return Results.NotFound();
    }
}