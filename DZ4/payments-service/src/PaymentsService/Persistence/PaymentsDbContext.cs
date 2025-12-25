using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain;

namespace PaymentsService.Persistence;

/// Определяет accounts, ledger, inbox, outbox.
public sealed class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LedgerEntry> Ledger => Set<LedgerEntry>();

    public DbSet<InboxEvent> Inbox => Set<InboxEvent>();
    public DbSet<OutboxEvent> Outbox => Set<OutboxEvent>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(b =>
        {
            b.ToTable("accounts");
            b.HasKey(x => x.UserId);
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.Balance).HasColumnName("balance").HasColumnType("numeric(18,2)");
            b.Property(x => x.Version).HasColumnName("version");
            
            b.Property(x => x.Version).IsConcurrencyToken();
        });

        modelBuilder.Entity<LedgerEntry>(b =>
        {
            b.ToTable("ledger");
            b.HasKey(x => x.TxId);

            b.Property(x => x.TxId).HasColumnName("tx_id");
            b.Property(x => x.OrderId).HasColumnName("order_id");
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.Type).HasColumnName("type");
            b.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(18,2)");
            b.Property(x => x.Status).HasColumnName("status");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasIndex(x => new { x.OrderId, x.Type }).IsUnique();
            
        });
        modelBuilder.Entity<InboxEvent>(b =>
        {
            b.ToTable("inbox");
            b.HasKey(x => x.EventId);
            b.Property(x => x.EventId).HasColumnName("event_id");
            b.Property(x => x.ReceivedAt).HasColumnName("received_at");
        });

        modelBuilder.Entity<OutboxEvent>(b =>
        {
            b.ToTable("outbox");
            b.HasKey(x => x.EventId);

            b.Property(x => x.EventId).HasColumnName("event_id");
            b.Property(x => x.EventType).HasColumnName("event_type");
            b.Property(x => x.PayloadJson).HasColumnName("payload_json").HasColumnType("jsonb");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.PublishedAt).HasColumnName("published_at");
            b.Property(x => x.PublishAttempts).HasColumnName("publish_attempts");

            b.HasIndex(x => x.PublishedAt);
        });
    }
}