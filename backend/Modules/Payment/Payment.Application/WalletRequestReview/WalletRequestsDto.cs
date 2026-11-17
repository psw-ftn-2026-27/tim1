namespace Payment.Application.WalletRequestReview;

public sealed record WalletRequestsDto(
    List<WalletRequestReviewDto> Pending,
    List<WalletRequestReviewDto> Decided);
