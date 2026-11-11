namespace Payment.Application.WalletFunding;

public sealed record WalletRequestDto(
    Guid Id,
    WalletRequestType Type,
    decimal Amount,
    WalletRequestStatus Status,
    DateTime CreatedAt);
