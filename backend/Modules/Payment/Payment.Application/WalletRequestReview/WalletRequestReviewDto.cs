using Payment.Application.Wallets;

namespace Payment.Application.WalletRequestReview;

public sealed record WalletRequestReviewDto(
    Guid Id,
    WalletRequestType Type,
    Guid SubmitterId,
    decimal Amount,
    DateTime CreatedAt,
    WalletRequestStatus Status,
    string? RejectionReason);
