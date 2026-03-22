namespace OrderService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, string key, T @event, CancellationToken ct = default);
}