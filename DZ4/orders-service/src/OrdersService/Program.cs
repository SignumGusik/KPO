using Microsoft.EntityFrameworkCore;
using OrdersService.Persistence;
using OrdersService.Outbox;
using OrdersService.Messaging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrdersDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("OrdersDb");
    opt.UseNpgsql(cs);
});

builder.Services.Configure<OrdersService.Messaging.RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq")
);

builder.Services.AddHostedService<OutboxPublisher>();
builder.Services.AddHostedService<PaymentResultConsumer>();
builder.Services.AddSingleton<WsOrderHub>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}
app.UseWebSockets();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapGet("/orders/ws", async (HttpContext ctx, WsOrderHub hub) =>
{
    if (!ctx.WebSockets.IsWebSocketRequest)
        return Results.BadRequest("WebSocket only");

    var ws = await ctx.WebSockets.AcceptWebSocketAsync();

    var buf = new byte[8 * 1024];
    var ms = new MemoryStream();

    try
    {
        while (ws.State == WebSocketState.Open && !ctx.RequestAborted.IsCancellationRequested)
        {
            WebSocketReceiveResult res;

            try
            {
                res = await ws.ReceiveAsync(new ArraySegment<byte>(buf), ctx.RequestAborted);
            }
            catch (OperationCanceledException)
            {
                break; // запрос отменён
            }
            catch (WebSocketException)
            {
                break; // клиент оборвал выходим
            }

            if (res.MessageType == WebSocketMessageType.Close)
                break;

            // Сборка сообщения
            ms.Write(buf, 0, res.Count);

            // Защита: если сообщение растёт слишком сильно — сбросим (чтобы не допустить роста памяти)
            const long maxMessageSize = 256 * 1024; // 256 KB
            if (ms.Length > maxMessageSize)
            {
                // очистим и продолжим
                ms.SetLength(0);
                continue;
            }

            if (!res.EndOfMessage)
                continue;

            var text = Encoding.UTF8.GetString(ms.ToArray());
            ms.SetLength(0);

            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;

                if (root.TryGetProperty("type", out var t) && t.GetString() == "subscribe"
                    && root.TryGetProperty("orderId", out var oid))
                {
                    var orderId = oid.GetString();
                    if (!string.IsNullOrWhiteSpace(orderId))
                        hub.Subscribe(orderId!, ws);
                }
            }
            catch
            {
                // плохой JSON/сообщение — игнорируем, соединение не рвём
            }
        }
    }
    finally
    {
        // Гарантированно удалить все подписки и освободить буфер
        hub.UnsubscribeAll(ws);

        try
        {
            if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
        }
        catch
        {
            // клиент уже оборвал — ок
        }

        try { ms.Dispose(); } catch { }
        ws.Dispose();
    }

    return Results.Empty;
});

app.Run();