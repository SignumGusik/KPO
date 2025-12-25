using System.ComponentModel.DataAnnotations;

namespace OrdersService.Domain;

public sealed class OutboxEvent
{
    [Key]
    public Guid EventId { get; set; }

    public string EventType { get; set; } = default!;
    public string PayloadJson { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public int PublishAttempts { get; set; }
}