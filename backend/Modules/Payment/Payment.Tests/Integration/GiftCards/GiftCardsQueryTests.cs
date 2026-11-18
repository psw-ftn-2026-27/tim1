using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Xunit;

namespace Payment.Tests.Integration.GiftCards;

public class GiftCardsQueryTests : BaseIntegrationTest
{
    public GiftCardsQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Customer_sees_the_gift_cards_they_created_with_their_status_but_not_others()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var giftCards = await client.GetFromJsonAsync<PageResult<GiftCardDto>>("/api/payment/gift-cards/created", JsonOptions);

        giftCards!.Items.Should().Contain(giftCard =>
            giftCard.Id == GiftCardSeed.ExplorersGiftCardForSecondExplorer.Id
            && giftCard.Amount == GiftCardSeed.ExplorersGiftCardForSecondExplorer.Amount
            && giftCard.Message == GiftCardSeed.ExplorersGiftCardForSecondExplorer.Message
            && giftCard.Status == GiftCardStatus.Valid
            && giftCard.RecipientDisplayName != "");
        giftCards.Items.Should().NotContain(giftCard => giftCard.Id == GiftCardSeed.SecondExplorersGiftCardForExplorer.Id);
        giftCards.Items.Should().NotContain(giftCard => giftCard.Id == GiftCardSeed.SecondExplorersGiftCardForThirdExplorer.Id);
    }

    [Fact]
    public async Task Customer_sees_the_gift_cards_they_received_with_their_status_but_not_others()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var giftCards = await client.GetFromJsonAsync<PageResult<GiftCardDto>>("/api/payment/gift-cards/received", JsonOptions);

        giftCards!.Items.Should().Contain(giftCard =>
            giftCard.Id == GiftCardSeed.SecondExplorersGiftCardForExplorer.Id
            && giftCard.Status == GiftCardStatus.Valid
            && giftCard.CreatorDisplayName != "");
        giftCards.Items.Should().NotContain(giftCard => giftCard.Id == GiftCardSeed.ExplorersGiftCardForSecondExplorer.Id);
        giftCards.Items.Should().NotContain(giftCard => giftCard.Id == GiftCardSeed.SecondExplorersGiftCardForThirdExplorer.Id);
    }

    [Fact]
    public async Task Created_gift_cards_come_in_pages_with_the_total_of_all_of_them()
    {
        var client = Factory.CreateClientFor(DemoUsers.SecondExplorer, "explorer");
        var expectedTotal = GiftCardSeed.All.Count(giftCard => giftCard.CreatorId == DemoUsers.SecondExplorer);

        var giftCards = await client.GetFromJsonAsync<PageResult<GiftCardDto>>(
            "/api/payment/gift-cards/created?page=1&pageSize=1", JsonOptions);

        giftCards!.Items.Should().HaveCount(1);
        giftCards.TotalCount.Should().Be(expectedTotal);
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

    [Fact]
    public async Task Recipient_search_finds_other_users_but_never_the_buyer()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var recipients = await client.GetFromJsonAsync<PageResult<RecipientDto>>(
            "/api/payment/gift-cards/recipients?text=&page=1&pageSize=50", JsonOptions);

        recipients!.Items.Should().Contain(recipient => recipient.Id == DemoUsers.SecondExplorer);
        recipients.Items.Should().NotContain(recipient => recipient.Id == DemoUsers.Explorer);
    }

    [Fact]
    public async Task Recipient_search_that_finds_only_the_buyer_returns_nothing()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var recipients = await client.GetFromJsonAsync<PageResult<RecipientDto>>(
            "/api/payment/gift-cards/recipients?text=explorer1@explorer.dev&page=1&pageSize=50", JsonOptions);

        recipients!.Items.Should().BeEmpty();
        recipients.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Recipient_search_reports_the_same_total_on_every_page_whether_or_not_the_buyer_is_on_it()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        var otherClient = Factory.CreateClientFor(DemoUsers.SecondExplorer, "explorer");

        var firstPage = await client.GetFromJsonAsync<PageResult<RecipientDto>>(
            "/api/payment/gift-cards/recipients?text=explorer&page=1&pageSize=1", JsonOptions);
        var secondPage = await client.GetFromJsonAsync<PageResult<RecipientDto>>(
            "/api/payment/gift-cards/recipients?text=explorer&page=2&pageSize=1", JsonOptions);
        var otherBuyersFirstPage = await otherClient.GetFromJsonAsync<PageResult<RecipientDto>>(
            "/api/payment/gift-cards/recipients?text=explorer&page=1&pageSize=1", JsonOptions);

        firstPage!.TotalCount.Should().Be(secondPage!.TotalCount);
        firstPage.TotalCount.Should().Be(otherBuyersFirstPage!.TotalCount);
        firstPage.Items.Concat(secondPage.Items).Should().NotContain(recipient => recipient.Id == DemoUsers.Explorer);
    }
}
