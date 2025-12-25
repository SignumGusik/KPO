using System.ComponentModel.DataAnnotations;

namespace OrdersService.Domain;

public sealed class InboxEvent
{
    [Key]
    public Guid EventId { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }
}