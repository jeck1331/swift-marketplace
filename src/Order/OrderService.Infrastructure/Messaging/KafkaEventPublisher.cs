using System.Text.Json;
using Confluent.Kafka;
using OrderService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OrderService.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(
        IConfiguration configuration,
        ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = configuration.GetConnectionString("Kafka"),
            Acks = Acks.All,             // ждём подтверждения от всех реплик
            EnableIdempotence = true,     // exactly-once на уровне producer
            MessageSendMaxRetries = 3
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(
        string topic, string key, T @event, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(@event);

        _logger.LogInformation(
            "Publishing to {Topic} key={Key}: {Event}",
            topic, key, json);

        var message = new Message<string, string>
        {
            Key = key,
            Value = json
        };

        var result = await _producer.ProduceAsync(topic, message, ct);

        _logger.LogInformation(
            "Published to {Topic} partition={Partition} offset={Offset}",
            topic, result.Partition.Value, result.Offset.Value);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}