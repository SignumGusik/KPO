using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace OrdersService.Messaging;

/// - Хранит маппинг orderId - набор WebSocket соединений
/// - Позволяет подписываться на orderId и отписываться при закрытии
/// - PublishAsync рассылает JSON { orderId, status } всем подписчикам
public sealed class WsOrderHub
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, byte>> _subs = new();

    /// Подписывает сокет на обновления для orderId.
    public void Subscribe(string orderId, WebSocket ws)
    {
        var set = _subs.GetOrAdd(orderId, _ => new ConcurrentDictionary<WebSocket, byte>());
        set.TryAdd(ws, 0);
    }

    /// Удаляет сокет из всех подписок (вызывается при закрытии соединения).
    public void UnsubscribeAll(WebSocket ws)
    {
        foreach (var kv in _subs)
            kv.Value.TryRemove(ws, out _);
    }

    /// Отправляет всем подписчикам orderId сообщение с новым статусом.
    /// Небольшая защита: если отправка падает, сокет удаляется.
    public async Task PublishAsync(string orderId, string status, CancellationToken ct = default)
    {
        if (!_subs.TryGetValue(orderId, out var set)) return;

        var payload = JsonSerializer.Serialize(new { orderId, status });
        var bytes = Encoding.UTF8.GetBytes(payload);
        var seg = new ArraySegment<byte>(bytes);

        foreach (var ws in set.Keys)
        {
            try
            {
                if (ws.State == WebSocketState.Open)
                    await ws.SendAsync(seg, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: ct);
            }
            catch
            {
                set.TryRemove(ws, out _);
            }
        }
    }
}