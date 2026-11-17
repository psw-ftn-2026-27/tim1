namespace Payment.Application.GiftCards;

public sealed record GiftCardsDto(List<GiftCardDto> Created, List<GiftCardDto> Received);
