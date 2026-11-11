namespace Payment.Application.WalletFunding;

public sealed record WalletOverviewDto(
    decimal AvailableBalance,
    decimal ReservedBalance,
    List<WalletTransactionDto> Transactions,
    List<WalletRequestDto> Requests);
