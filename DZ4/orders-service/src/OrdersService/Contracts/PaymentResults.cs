namespace OrdersService.Contracts;

public sealed record PaymentSucceeded(
    Guid EventId,
    Guid OrderId
);

public sealed record PaymentFailed(
    Guid EventId,
    Guid OrderId,
    string Reason
);