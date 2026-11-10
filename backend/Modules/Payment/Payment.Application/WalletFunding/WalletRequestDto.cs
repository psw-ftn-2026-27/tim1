using Payment.Domain.TopUpRequests;

namespace Payment.Application.WalletFunding;

public sealed record WalletRequestDto(
    Guid Id,
    WalletRequestType Type,
    decimal Amount,
    TopUpRequestStatus Status,
    DateTime CreatedAt);
