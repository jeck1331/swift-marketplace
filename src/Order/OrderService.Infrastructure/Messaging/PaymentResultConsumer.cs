using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Contracts.Events;

namespace OrderService.Infrastructure.Messaging;

public class PaymentResultConsumer: BackgroundService, IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<PaymentResultConsumer> _logger;

    public PaymentResultConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<PaymentResultConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration.GetConnectionString("Kafka"),
            GroupId = "order-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false    // ручной commit после обработки
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _consumer.Subscribe(new[]
        {
            KafkaTopics.PaymentSucceeded,
            KafkaTopics.PaymentFailed
        });

        _logger.LogInformation("PaymentResultConsumer started");

        // Kafka consumer — блокирующий, выносим в отдельный поток
        await Task.Run(() => ConsumeLoop(ct), ct);
    }

    private void ConsumeLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(ct);

                _logger.LogInformation(
                    "Consumed from {Topic}: {Message}",
                    result.Topic, result.Message.Value);

                using var scope = _scopeFactory.CreateScope();
                var orderService = scope.ServiceProvider
                    .GetRequiredService<Application.Services.OrderService>();

                switch (result.Topic)
                {
                    case KafkaTopics.PaymentSucceeded:
                        var succeeded = JsonSerializer
                            .Deserialize<PaymentSucceededEvent>(result.Message.Value)!;

                        orderService.HandlePaymentSucceededAsync(
                            succeeded.OrderId, succeeded.PaidAt, ct)
                            .GetAwaiter().GetResult();
                        break;

                    case KafkaTopics.PaymentFailed:
                        var failed = JsonSerializer
                            .Deserialize<PaymentFailedEvent>(result.Message.Value)!;

                        orderService.HandlePaymentFailedAsync(
                            failed.OrderId, failed.Reason, ct)
                            .GetAwaiter().GetResult();
                        break;
                }

                // Commit только после успешной обработки
                _consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}