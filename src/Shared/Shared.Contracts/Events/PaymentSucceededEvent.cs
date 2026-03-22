namespace Shared.Contracts.Events;

public class PaymentSucceededEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
}