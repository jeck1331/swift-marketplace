namespace Shared.Contracts.Events;

public class OrderCancelledEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public List<OrderItemEvent> Items { get; set; } = [];
    public string Reason { get; set; } = null!;
    public DateTime CancelledAt { get; set; }
}