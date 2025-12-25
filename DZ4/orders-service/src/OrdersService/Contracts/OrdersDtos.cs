namespace OrdersService.Contracts;

public sealed record CreateOrderRequest(string UserId, decimal Amount);
public sealed record CreateOrderResponse(Guid OrderId);

public sealed record OrderResponse(Guid OrderId, string UserId, decimal Amount, string Status, DateTimeOffset CreatedAt);