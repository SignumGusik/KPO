using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentsService.Persistence;
using RabbitMQ.Client;

namespace PaymentsService.Outbox;

public sealed class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Messaging.RabbitMqOptions _opt;
    private readonly ILogger<OutboxPublisher> _log;

    private IConnection? _conn;
    private IModel? _ch;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IOptions<Messaging.RabbitMqOptions> opt,
        ILogger<OutboxPublisher> log)
    {
        _scopeFactory = scopeFactory;
        _opt = opt.Value;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_ch is null || _conn is null || !_conn.IsOpen)
                    EnsureRabbit();

                await PublishOnce(stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Payments outbox loop error, retrying...");
                try { RecreateRabbit(); } catch { /* проглотили, попробуем позже */ }
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task PublishOnce(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        var batch = await db.Outbox
            .Where(x => x.PublishedAt == null)
            .OrderBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        if (batch.Count == 0) return;

        foreach (var msg in batch)
        {
            try
            {
                PublishToRabbit(msg.PayloadJson, msg.EventType, msg.EventId);
                msg.PublishedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to publish payments outbox event {EventId}", msg.EventId);
                msg.PublishAttempts += 1;
                await db.SaveChangesAsync(ct);
            }
        }
    }

    private void PublishToRabbit(string payloadJson, string eventType, Guid eventId)
    {
        if (_ch is null) throw new InvalidOperationException("RabbitMQ channel not initialized");
        
        using var doc = JsonDocument.Parse(payloadJson);
        var routingKey = doc.RootElement.GetProperty("routingKey").GetString()
                         ?? throw new InvalidOperationException("routingKey missing");

        var payloadElement = doc.RootElement.GetProperty("payload");
        var payload = Encoding.UTF8.GetBytes(payloadElement.GetRawText());

        var props = _ch.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.Type = eventType;
        props.MessageId = eventId.ToString();

        _ch.BasicPublish(_opt.Exchange, routingKey, false, props, payload);
    }

    private void EnsureRabbit()
    {
        var factory = new ConnectionFactory
        {
            HostName = _opt.Host,
            Port = _opt.Port,
            UserName = _opt.User,
            Password = _opt.Pass
        };

        _conn = factory.CreateConnection();
        _ch = _conn.CreateModel();
        _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);

        _log.LogInformation("RabbitMQ connected for payments outbox publisher");
    }

    private void RecreateRabbit()
    {
        try { _ch?.Dispose(); } catch { }
        try { _conn?.Dispose(); } catch { }
        _ch = null;
        _conn = null;
        EnsureRabbit();
    }

    public override void Dispose()
    {
        try { _ch?.Dispose(); } catch { }
        try { _conn?.Dispose(); } catch { }
        base.Dispose();
    }
}