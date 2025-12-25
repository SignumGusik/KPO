using System.ComponentModel.DataAnnotations;

namespace OrdersService.Domain;

public enum OrderStatus
{
    PAYMENT_PENDING = 1,
    PAID = 2,
    PAYMENT_FAILED = 3
}

public sealed class Order
{
    [Key]
    public Guid OrderId { get; set; }

    public string UserId { get; set; } = default!;

    public decimal Amount { get; set; }

    public OrderStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}