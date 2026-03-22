using OrderService.Application.Interfaces;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;

namespace OrderService.Infrastructure.Grpc;

public class CatalogGrpcClient : ICatalogGrpcClient, IDisposable
{
    private readonly CatalogGrpc.CatalogGrpcClient _client;
    private readonly GrpcChannel _channel;
    private readonly ILogger<CatalogGrpcClient> _logger;

    public CatalogGrpcClient(
        IConfiguration configuration,
        ILogger<CatalogGrpcClient> logger)
    {
        _logger = logger;

        var address = configuration["CatalogService:GrpcUrl"]
            ?? "http://localhost:5010";

        _channel = GrpcChannel.ForAddress(address);
        _client = new CatalogGrpc.CatalogGrpcClient(_channel);
    }

    public async Task<StockCheckResult> CheckStockAsync(
        Guid productId, int quantity, CancellationToken ct = default)
    {
        _logger.LogInformation("gRPC CheckStock: {ProductId} qty={Quantity}",
            productId, quantity);

        var response = await _client.CheckStockAsync(
            new CheckStockRequest
            {
                ProductId = productId.ToString(),
                Quantity = quantity
            },
            cancellationToken: ct);

        return new StockCheckResult
        {
            IsAvailable = response.IsAvailable,
            AvailableQuantity = response.AvailableQuantity,
            Price = (decimal)response.Price,
            ProductName = response.ProductName
        };
    }

    public async Task<bool> ReserveStockAsync(
        Guid productId, int quantity, Guid orderId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "gRPC ReserveStock: {ProductId} qty={Quantity} order={OrderId}",
            productId, quantity, orderId);

        var response = await _client.ReserveStockAsync(
            new ReserveStockRequest
            {
                ProductId = productId.ToString(),
                Quantity = quantity,
                OrderId = orderId.ToString()
            },
            cancellationToken: ct);

        return response.Success;
    }

    public async Task ReleaseStockAsync(
        Guid productId, int quantity, Guid orderId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "gRPC ReleaseStock: {ProductId} qty={Quantity} order={OrderId}",
            productId, quantity, orderId);

        await _client.ReleaseStockAsync(
            new ReleaseStockRequest
            {
                ProductId = productId.ToString(),
                Quantity = quantity,
                OrderId = orderId.ToString()
            },
            cancellationToken: ct);
    }

    public void Dispose() => _channel.Dispose();
}