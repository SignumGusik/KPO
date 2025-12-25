using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersService.Contracts;
using OrdersService.Domain;
using OrdersService.Persistence;
using System.Text.Json;

namespace OrdersService.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _db;

    public OrdersController(OrdersDbContext db) => _db = db;

    // POST /orders
    // вход
    // сохраняем заказ со статусом PAYMENT_PENDING, возвращаем orderId
    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.UserId))
            return BadRequest(new { error = "userId is required" });

        if (req.Amount <= 0)
            return BadRequest(new { error = "amount must be > 0" });

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UserId = req.UserId,
            Amount = req.Amount,
            Status = OrderStatus.PAYMENT_PENDING,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var ev = new PaymentRequested(
            EventId: Guid.NewGuid(),
            OrderId: order.OrderId,
            UserId: order.UserId,
            Amount: order.Amount
        );

        _db.Orders.Add(order);
        _db.Outbox.Add(new OutboxEvent
        {
            EventId = ev.EventId,
            EventType = nameof(PaymentRequested),
            PayloadJson = JsonSerializer.Serialize(ev),
            CreatedAt = DateTimeOffset.UtcNow,
            PublishedAt = null,
            PublishAttempts = 0
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new CreateOrderResponse(order.OrderId));
    }

    // GET /orders?userId=u1
    // список заказов пользователя
    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> List([FromQuery] string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { error = "userId query param is required" });

        var orders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderResponse(
                o.OrderId,
                o.UserId,
                o.Amount,
                o.Status.ToString(),
                o.CreatedAt
            ))
            .ToListAsync(ct);

        return Ok(orders);
    }

    // GET /orders/{orderId}
    // статус заказа (и данные)
    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderResponse>> Get(Guid orderId, CancellationToken ct)
    {
        var o = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
        if (o is null) return NotFound();

        return Ok(new OrderResponse(o.OrderId, o.UserId, o.Amount, o.Status.ToString(), o.CreatedAt));
    }
}