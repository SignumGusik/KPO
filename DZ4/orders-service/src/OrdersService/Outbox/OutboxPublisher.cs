using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrdersService.Domain;
using OrdersService.Messaging;
using OrdersService.Persistence;
using RabbitMQ.Client;

namespace OrdersService.Outbox;

public sealed class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _opt;
    private readonly ILogger<OutboxPublisher> _log;

    private IConnection? _conn;
    private IModel? _ch;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> opt,
        ILogger<OutboxPublisher> log)
    {
        _scopeFactory = scopeFactory;
        _opt = opt.Value;
        _log = log;
    }

    /// Main loop: читает батчи из outbox и публикует в RabbitMQ, помечая PublishedAt при успехе.
    /// При ошибке увеличивает PublishAttempts и пытается восстановить соединение.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Берём батч непубликованных
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                var batch = await db.Outbox
                    .Where(x => x.PublishedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                if (batch.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                // Подключаемся к Rabbit когда есть что публиковать
                if (_conn is null || !_conn.IsOpen || _ch is null || !_ch.IsOpen)
                    EnsureRabbit();

                foreach (var msg in batch)
                {
                    try
                    {
                        PublishToRabbit(msg);

                        msg.PublishedAt = DateTimeOffset.UtcNow;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(ex, "Failed to publish outbox event {EventId}", msg.EventId);
                        msg.PublishAttempts += 1;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Orders outbox publish loop error, will retry");
                try { RecreateRabbit(); } catch { }
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }

    /// Публикует одно сообщение в exchange с заданным routing key (PaymentRequested).
    /// Сообщения помечаются persistent, чтобы брокер сохранял их при перезапуске.
    private void PublishToRabbit(OutboxEvent msg)
    {
        if (_ch is null) throw new InvalidOperationException("RabbitMQ channel not initialized");

        var props = _ch.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.Type = msg.EventType;
        props.MessageId = msg.EventId.ToString();

        var body = Encoding.UTF8.GetBytes(msg.PayloadJson);

        _ch.BasicPublish(
            exchange: _opt.Exchange,
            routingKey: _opt.RoutingKeyPaymentRequested, // <-- ВАЖНО
            mandatory: false,
            basicProperties: props,
            body: body
        );

        _log.LogInformation("Outbox published EventId={EventId} rk={RoutingKey}", msg.EventId, _opt.RoutingKeyPaymentRequested);
    }

    /// Создаёт соединение и канал к RabbitMQ и декларирует exchange.
    private void EnsureRabbit()
    {
        var factory = new ConnectionFactory
        {
            HostName = _opt.Host,
            Port = _opt.Port,
            UserName = _opt.User,
            Password = _opt.Pass,
            DispatchConsumersAsync = true
        };

        _conn = factory.CreateConnection();
        _ch = _conn.CreateModel();

        _ch.ExchangeDeclare(exchange: _opt.Exchange, type: ExchangeType.Topic, durable: true, autoDelete: false);

        _log.LogInformation("RabbitMQ connected: {Host}:{Port}, exchange={Exchange}", _opt.Host, _opt.Port, _opt.Exchange);
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