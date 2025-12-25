using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentsService.Contracts;
using PaymentsService.Domain;
using PaymentsService.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaymentsService.Messaging;

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "guest";
    public string Pass { get; set; } = "guest";
    public string Exchange { get; set; } = "events";

    public string QueuePaymentRequested { get; set; } = "payments.payment-requested.q";
    public string RoutingKeyPaymentRequested { get; set; } = "payments.payment-requested";

    public string RoutingKeyPaymentSucceeded { get; set; } = "orders.payment-succeeded";
    public string RoutingKeyPaymentFailed { get; set; } = "orders.payment-failed";
}
/// Consumer в PaymentsService, который принимает PaymentRequested события.
/// Алгоритм:
/// 1) Inbox dedup по EventId
/// 2) Проверка существующего debit по orderId (идемпотентность)
/// 3) Если нужно — списываем/отказываем, записываем ledger и enqueue outbox
/// 4) Сохраняем всё в одной транзакции (transactional inbox + business)
/// Реализован цикл переподключения к RabbitMQ для устойчивости.
public sealed class PaymentRequestedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _opt;
    private readonly ILogger<PaymentRequestedConsumer> _log;

    private IConnection? _conn;
    private IModel? _ch;

    public PaymentRequestedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> opt,
        ILogger<PaymentRequestedConsumer> log)
    {
        _scopeFactory = scopeFactory;
        _opt = opt.Value;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Цикл позволяет переподключаться при сбоях брокера/сети
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
                        var msg = JsonSerializer.Deserialize<PaymentRequested>(json)
                                  ?? throw new InvalidOperationException("Cannot deserialize PaymentRequested");

                        await Handle(msg, stoppingToken);

                        _ch!.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(ex, "Error while handling PaymentRequested, will requeue");
                        try { _ch!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true); } catch { }
                    }
                };

                _ch!.BasicConsume(queue: _opt.QueuePaymentRequested, autoAck: false, consumer: consumer);

                _log.LogInformation("PaymentRequestedConsumer started and listening. Queue={Queue}", _opt.QueuePaymentRequested);

                // Ждём отмены или падения соединения
                while (!stoppingToken.IsCancellationRequested && _conn != null && _conn.IsOpen)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Завершение сервиса
                break;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "PaymentRequestedConsumer error, will attempt reconnect");
                try { RecreateRabbit(); } catch { }
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    // Handle реализует транзакционный сценарий списания/неуспеха и постановки события в outbox (wrap with routingKey)
    private async Task Handle(PaymentRequested msg, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Inbox dedup по eventId
        var already = await db.Inbox.AsNoTracking().AnyAsync(x => x.EventId == msg.EventId, ct);
        if (already)
        {
            await tx.CommitAsync(ct);
            return; 
        }

        // ложим eventId в inbox
        db.Inbox.Add(new InboxEvent
        {
            EventId = msg.EventId,
            ReceivedAt = DateTimeOffset.UtcNow
        });

        // Проверяем, было ли уже списание по orderId (DEBIT)
        var existingDebit = await db.Ledger
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderId == msg.OrderId && x.Type == LedgerType.DEBIT, ct);

        if (existingDebit is not null)
        {
            // уже обрабатывали этот orderId - баланс не трогаем
            if (existingDebit.Status == LedgerStatus.SUCCESS)
            {
                await EnqueueOutbox(db,
                    eventType: nameof(PaymentSucceeded),
                    routingKey: _opt.RoutingKeyPaymentSucceeded,
                    payload: new PaymentSucceeded(Guid.NewGuid(), msg.OrderId, msg.UserId, msg.Amount),
                    ct: ct);
            }
            else
            {
                await EnqueueOutbox(db,
                    eventType: nameof(PaymentFailed),
                    routingKey: _opt.RoutingKeyPaymentFailed,
                    payload: new PaymentFailed(Guid.NewGuid(), msg.OrderId, msg.UserId, msg.Amount, "Already failed"),
                    ct: ct);
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return;
        }

        // Если списания ещё нет — проверяем баланс
        var acc = await db.Accounts.FirstOrDefaultAsync(a => a.UserId == msg.UserId, ct);
        if (acc is null)
        {
            db.Ledger.Add(new LedgerEntry
            {
                TxId = Guid.NewGuid(),
                OrderId = msg.OrderId,
                UserId = msg.UserId,
                Type = LedgerType.DEBIT,
                Amount = msg.Amount,
                Status = LedgerStatus.FAILED,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await EnqueueOutbox(db,
                eventType: nameof(PaymentFailed),
                routingKey: _opt.RoutingKeyPaymentFailed,
                payload: new PaymentFailed(Guid.NewGuid(), msg.OrderId, msg.UserId, msg.Amount, "Account not found"),
                ct: ct);

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return;
        }

        if (acc.Balance < msg.Amount)
        {
            // недостаточно денег - DEBIT FAILED, баланс не меняем
            db.Ledger.Add(new LedgerEntry
            {
                TxId = Guid.NewGuid(),
                OrderId = msg.OrderId,
                UserId = msg.UserId,
                Type = LedgerType.DEBIT,
                Amount = msg.Amount,
                Status = LedgerStatus.FAILED,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await EnqueueOutbox(db,
                eventType: nameof(PaymentFailed),
                routingKey: _opt.RoutingKeyPaymentFailed,
                payload: new PaymentFailed(Guid.NewGuid(), msg.OrderId, msg.UserId, msg.Amount, "Insufficient funds"),
                ct: ct);
        }
        else
        {
            db.Ledger.Add(new LedgerEntry
            {
                TxId = Guid.NewGuid(),
                OrderId = msg.OrderId,
                UserId = msg.UserId,
                Type = LedgerType.DEBIT,
                Amount = msg.Amount,
                Status = LedgerStatus.SUCCESS,
                CreatedAt = DateTimeOffset.UtcNow
            });

            acc.Balance -= msg.Amount;
            acc.Version += 1;

            await EnqueueOutbox(db,
                eventType: nameof(PaymentSucceeded),
                routingKey: _opt.RoutingKeyPaymentSucceeded,
                payload: new PaymentSucceeded(Guid.NewGuid(), msg.OrderId, msg.UserId, msg.Amount),
                ct: ct);
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    // Вставляет в таблицу outbox wrapper { routingKey, payload } - Payments.OutboxPublisher ожидает такой формат
    private static Task EnqueueOutbox<T>(
        PaymentsDbContext db,
        string eventType,
        string routingKey,
        T payload,
        CancellationToken ct)
    {
        var wrapper = new
        {
            routingKey,
            payload
        };

        db.Outbox.Add(new OutboxEvent
        {
            EventId = Guid.NewGuid(),
            EventType = eventType,
            PayloadJson = JsonSerializer.Serialize(wrapper),
            CreatedAt = DateTimeOffset.UtcNow,
            PublishedAt = null,
            PublishAttempts = 0
        });

        return Task.CompletedTask;
    }

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
            queue: _opt.QueuePaymentRequested,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _ch.QueueBind(
            queue: _opt.QueuePaymentRequested,
            exchange: _opt.Exchange,
            routingKey: _opt.RoutingKeyPaymentRequested);

        _ch.BasicQos(0, prefetchCount: 1, global: false);

        _log.LogInformation("RabbitMQ connected for payments consumer. Queue={Queue}", _opt.QueuePaymentRequested);
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