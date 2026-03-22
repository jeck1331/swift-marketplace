namespace OrderService.Domain.Entities;

public enum OrderStatus
{
    Pending = 0,
    StockReserved = 1,
    Paid = 2,
    Cancelled = 3,
    Failed = 4
}