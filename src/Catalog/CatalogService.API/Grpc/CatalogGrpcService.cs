using CatalogService.Application.Interfaces;
using global::Grpc.Core;

namespace CatalogService.API.Grpc;

public class CatalogGrpcService: CatalogGrpc.CatalogGrpcBase
{
    private readonly IStockRepository _stockRepo;
    private readonly IProductRepository _productRepo;
    private readonly ILogger<CatalogGrpcService> _logger;

    public CatalogGrpcService(
        IStockRepository stockRepo,
        IProductRepository productRepo,
        ILogger<CatalogGrpcService> logger)
    {
        _stockRepo = stockRepo;
        _productRepo = productRepo;
        _logger = logger;
    }

    public override async Task<CheckStockResponse> CheckStock(
        CheckStockRequest request, ServerCallContext context)
    {
        var productId = Guid.Parse(request.ProductId);

        var product = await _productRepo.GetByIdAsync(productId, context.CancellationToken);
        if (product is null)
        {
            return new CheckStockResponse
            {
                IsAvailable = false,
                AvailableQuantity = 0
            };
        }

        var stock = await _stockRepo.GetByProductIdAsync(productId, context.CancellationToken);

        var available = stock?.Available ?? 0;

        return new CheckStockResponse
        {
            IsAvailable = available >= request.Quantity,
            AvailableQuantity = available,
            Price = (double)product.Price,
            ProductName = product.Name
        };
    }

    public override async Task<ReserveStockResponse> ReserveStock(
        ReserveStockRequest request, ServerCallContext context)
    {
        var productId = Guid.Parse(request.ProductId);

        _logger.LogInformation(
            "Reserving {Quantity} of product {ProductId} for order {OrderId}",
            request.Quantity, request.ProductId, request.OrderId);

        var success = await _stockRepo.ReserveAsync(
            productId, request.Quantity, context.CancellationToken);

        return new ReserveStockResponse
        {
            Success = success,
            Message = success ? "Reserved" : "Insufficient stock"
        };
    }

    public override async Task<ReleaseStockResponse> ReleaseStock(
        ReleaseStockRequest request, ServerCallContext context)
    {
        var productId = Guid.Parse(request.ProductId);

        _logger.LogInformation(
            "Releasing {Quantity} of product {ProductId} for order {OrderId}",
            request.Quantity, request.ProductId, request.OrderId);

        await _stockRepo.ReleaseReserveAsync(
            productId, request.Quantity, context.CancellationToken);

        return new ReleaseStockResponse { Success = true };
    }
}