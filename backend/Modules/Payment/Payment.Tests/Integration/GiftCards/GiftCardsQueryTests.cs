using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;
using Payment.Tests.Integration.Seeds;
using Xunit;

namespace Payment.Tests.Integration.GiftCards;

public class GiftCardsQueryTests : BaseIntegrationTest
{
    public GiftCardsQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Customer_sees_the_gift_cards_they_created_and_received_with_their_status_but_not_others()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var giftCards = await client.GetFromJsonAsync<GiftCardsDto>("/api/payment/gift-cards", JsonOptions);

        giftCards!.Created.Should().Contain(giftCard =>
            giftCard.Id == GiftCardSeed.ExplorersGiftCardForSecondExplorer.Id
            && giftCard.Amount == GiftCardSeed.ExplorersGiftCardForSecondExplorer.Amount
            && giftCard.Message == GiftCardSeed.ExplorersGiftCardForSecondExplorer.Message
            && giftCard.Status == GiftCardStatus.Valid
            && giftCard.RecipientDisplayName != "");
        giftCards.Received.Should().Contain(giftCard =>
            giftCard.Id == GiftCardSeed.SecondExplorersGiftCardForExplorer.Id
            && giftCard.Status == GiftCardStatus.Valid
            && giftCard.CreatorDisplayName != "");
        giftCards.Created.Should().NotContain(giftCard => giftCard.Id == GiftCardSeed.SecondExplorersGiftCardForExplorer.Id);
        giftCards.Received.Should().NotContain(giftCard => giftCard.Id == GiftCardSeed.ExplorersGiftCardForSecondExplorer.Id);
        giftCards.Created.Concat(giftCards.Received).Should().NotContain(giftCard =>
            giftCard.Id == GiftCardSeed.SecondExplorersGiftCardForThirdExplorer.Id);
    }

    [Fact]
    public async Task Customer_sees_how_many_valid_gift_cards_they_have_received()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        var expectedCount = GiftCardSeed.All.Count(giftCard =>
            giftCard.RecipientId == DemoUsers.Explorer && giftCard.StatusAt(DateTime.UtcNow) == GiftCardStatus.Valid);

        var count = await client.GetFromJsonAsync<int>("/api/payment/gift-cards/received/valid-count", JsonOptions);

        count.Should().Be(expectedCount);
    }
}
