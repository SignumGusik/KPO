namespace PaymentsService.Contracts;

public sealed record PaymentRequested(
    Guid EventId,
    Guid OrderId,
    string UserId,
    decimal Amount
);

public sealed record PaymentSucceeded(
    Guid EventId,
    Guid OrderId,
    string UserId,
    decimal Amount
);

public sealed record PaymentFailed(
    Guid EventId,
    Guid OrderId,
    string UserId,
    decimal Amount,
    string Reason
);