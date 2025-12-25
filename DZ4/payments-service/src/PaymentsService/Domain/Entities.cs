using System.ComponentModel.DataAnnotations;

namespace PaymentsService.Domain;

public sealed class Account
{
    [Key]
    public string UserId { get; set; } = default!;

    public decimal Balance { get; set; }
    
    public int Version { get; set; }
}

public enum LedgerType
{
    TOPUP = 1,
    DEBIT = 2
}

public enum LedgerStatus
{
    SUCCESS = 1,
    FAILED = 2
}

public sealed class LedgerEntry
{
    [Key]
    public Guid TxId { get; set; }

    public Guid? OrderId { get; set; } 
    public string UserId { get; set; } = default!;
    public LedgerType Type { get; set; }
    public decimal Amount { get; set; }
    public LedgerStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
public sealed class InboxEvent
{
    [Key]
    public Guid EventId { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }
}

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