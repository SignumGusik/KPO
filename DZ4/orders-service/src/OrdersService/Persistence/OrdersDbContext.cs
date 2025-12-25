using Microsoft.EntityFrameworkCore;
using OrdersService.Domain;

namespace OrdersService.Persistence;

/// Конфигурирует таблицы: orders, outbox, inbox и необходимые индексы.
public sealed class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxEvent> Outbox => Set<OutboxEvent>();
    public DbSet<InboxEvent> Inbox => Set<InboxEvent>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Конфигурация таблицы orders
        modelBuilder.Entity<Order>(b =>
        {
            b.ToTable("orders");
            b.HasKey(x => x.OrderId);

            b.Property(x => x.OrderId).HasColumnName("order_id");
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(18,2)");
            b.Property(x => x.Status).HasColumnName("status");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.CreatedAt);
        });
        // Outbox table для transactional outbox
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
        // Inbox table для idempotency
        modelBuilder.Entity<InboxEvent>(b =>
        {
            b.ToTable("inbox");
            b.HasKey(x => x.EventId);

            b.Property(x => x.EventId).HasColumnName("event_id");
            b.Property(x => x.ReceivedAt).HasColumnName("received_at");
        });
    }
}