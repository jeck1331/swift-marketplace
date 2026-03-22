namespace Shared.Contracts;

public static class KafkaTopics
{
    public const string OrderCreated = "order-created";
    public const string PaymentSucceeded = "payment-succeeded";
    public const string PaymentFailed = "payment-failed";
    public const string OrderCancelled = "order-cancelled";
}