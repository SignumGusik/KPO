using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrdersService.Contracts;
using OrdersService.Domain;
using OrdersService.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrdersService.Messaging;

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "guest";
    public string Pass { get; set; } = "guest";
    public string Exchange { get; set; } = "events";

    public string QueuePaymentResults { get; set; } = "orders.payment-results.q";
    public string RoutingKeyPaymentSucceeded { get; set; } = "orders.payment-succeeded";
    public string RoutingKeyPaymentFailed { get; set; } = "orders.payment-failed";

    // нужно для Orders Outbox Publisher (чтобы не ломать конфиг)
    public string RoutingKeyPaymentRequested { get; set; } = "payments.payment-requested";
}
/// Consumer для результатов платежей (PaymentSucceeded / PaymentFailed).
/// Слушает очередь orders.payment-results.q и обновляет статус заказа в Orders DB.
/// Применяет паттерн Inbox для дедупликации и отправляет WS-пуши после коммита.
/// Также реализует reconnect цикл для устойчивости к перезапуску брокера.
public sealed class PaymentResultConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _opt;
    private readonly ILogger<PaymentResultConsumer> _log;

    private IConnection? _conn;
    private IModel? _ch;

    private readonly WsOrderHub _hub;

    public PaymentResultConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> opt,
        ILogger<PaymentResultConsumer> log,
        WsOrderHub hub)
    {
        _scopeFactory = scopeFactory;
        _opt = opt.Value;
        _log = log;
        _hub = hub;
    }

    /// пытается подключиться к RabbitMQ, подписаться на очередь и обрабатывать сообщения.
    /// При ошибках пытается переподключиться с задержкой.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Цикл позволяет переподключаться в случае падения RabbitMQ/сети
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                EnsureRabbit();

                var consumer = new AsyncEventingBasicConsumer(_ch!);
                consumer.Received += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                        // Смотрим routingKey и выбираем DTO
                        var routingKey = ea.RoutingKey;

                        if (routingKey == _opt.RoutingKeyPaymentSucceeded)
                        {
                            var msg = JsonSerializer.Deserialize<PaymentSucceeded>(json)
                                      ?? throw new InvalidOperationException("Cannot deserialize PaymentSucceeded");
                            await HandleSucceeded(msg, stoppingToken);
                        }
                        else if (routingKey == _opt.RoutingKeyPaymentFailed)
                        {
                            var msg = JsonSerializer.Deserialize<PaymentFailed>(json)
                                      ?? throw new InvalidOperationException("Cannot deserialize PaymentFailed");
                            await HandleFailed(msg, stoppingToken);
                        }
                        else
                        {
                            _log.LogWarning("Unknown routingKey: {RoutingKey}", routingKey);
                        }

                        _ch!.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(ex, "Error while handling payment result, will requeue");
                        try { _ch!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true); } catch { }
                    }
                };

                _ch!.BasicConsume(queue: _opt.QueuePaymentResults, autoAck: false, consumer: consumer);

                _log.LogInformation("PaymentResultConsumer started and listening. Queue={Queue}", _opt.QueuePaymentResults);

                // Ждём отмены или падения соединения
                while (!stoppingToken.IsCancellationRequested && _conn != null && _conn.IsOpen)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // завершение сервиса
                break;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "PaymentResultConsumer error, will attempt reconnect");
                try { RecreateRabbit(); } catch { }
                // таймаут перед новой попыткой
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    private async Task HandleSucceeded(PaymentSucceeded msg, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        bool changed = false;

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var seen = await db.Inbox.AsNoTracking().AnyAsync(x => x.EventId == msg.EventId, ct);
        if (seen)
        {
            await tx.CommitAsync(ct);
            return;
        }

        db.Inbox.Add(new InboxEvent { EventId = msg.EventId, ReceivedAt = DateTimeOffset.UtcNow });

        var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == msg.OrderId, ct);
        if (order is null)
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return;
        }

        if (order.Status != OrderStatus.PAID)
        {
            order.Status = OrderStatus.PAID;
            changed = true;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // WS push после коммита
        if (changed)
            await _hub.PublishAsync(order.OrderId.ToString(), "PAID", ct);
    }

    private async Task HandleFailed(PaymentFailed msg, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        bool changed = false;

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var seen = await db.Inbox.AsNoTracking().AnyAsync(x => x.EventId == msg.EventId, ct);
        if (seen)
        {
            await tx.CommitAsync(ct);
            return;
        }

        db.Inbox.Add(new InboxEvent { EventId = msg.EventId, ReceivedAt = DateTimeOffset.UtcNow });

        var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == msg.OrderId, ct);
        if (order is null)
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return;
        }

        // если уже PAID — не откатываем
        if (order.Status != OrderStatus.PAID && order.Status != OrderStatus.PAYMENT_FAILED)
        {
            order.Status = OrderStatus.PAYMENT_FAILED;
            changed = true;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // WS push после коммита
        if (changed)
            await _hub.PublishAsync(order.OrderId.ToString(), "PAYMENT_FAILED", ct);
    }

    /// Создаёт и декларирует exchange/queue/bindings для получения результатов платежей.
    private void EnsureRabbit()
    {
        if (_conn != null && _conn.IsOpen && _ch != null && _ch.IsOpen) return;

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

        _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);

        _ch.QueueDeclare(
            queue: _opt.QueuePaymentResults,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _ch.QueueBind(_opt.QueuePaymentResults, _opt.Exchange, _opt.RoutingKeyPaymentSucceeded);
        _ch.QueueBind(_opt.QueuePaymentResults, _opt.Exchange, _opt.RoutingKeyPaymentFailed);

        _ch.BasicQos(0, prefetchCount: 1, global: false);

        _log.LogInformation("Orders payment result consumer ready. Queue={Queue}", _opt.QueuePaymentResults);
    }

    private void RecreateRabbit()
    {
        try { _ch?.Dispose(); } catch { }
        try { _conn?.Dispose(); } catch { }
        _ch = null;
        _conn = null;
    }

    public override void Dispose()
    {
        try { _ch?.Dispose(); } catch { }
        try { _conn?.Dispose(); } catch { }
        base.Dispose();
    }
}