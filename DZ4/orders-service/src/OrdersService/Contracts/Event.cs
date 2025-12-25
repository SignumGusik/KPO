namespace OrdersService.Contracts;

public sealed record PaymentRequested(
    Guid EventId,
    Guid OrderId,
    string UserId,
    decimal Amount
);