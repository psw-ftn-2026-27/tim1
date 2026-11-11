using Payment.Domain.Wallets;

namespace Payment.Application.WalletFunding;

public sealed record WalletTransactionDto(
    DateTime OccurredAt,
    TransactionType Type,
    decimal Amount,
    decimal BalanceAfter);
