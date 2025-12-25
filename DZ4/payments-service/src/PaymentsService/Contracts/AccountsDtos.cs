namespace PaymentsService.Contracts;

public sealed record CreateAccountRequest(string UserId);
public sealed record AccountResponse(string UserId, decimal Balance);

public sealed record TopUpRequest(decimal Amount);