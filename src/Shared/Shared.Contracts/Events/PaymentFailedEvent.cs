namespace Shared.Contracts.Events;

public class PaymentFailedEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime FailedAt { get; set; }
}