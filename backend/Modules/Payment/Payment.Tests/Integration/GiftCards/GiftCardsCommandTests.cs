using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCards;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.GiftCards;

public class GiftCardsCommandTests : BaseIntegrationTest
{
    public GiftCardsCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Customer_buys_a_gift_card_and_their_wallet_is_lowered_by_its_amount()
    {
        var client = Factory.CreateClientFor(DemoUsers.SecondExplorer, "explorer");
        var request = new PurchaseGiftCardDto(DemoUsers.ThirdExplorer, 120m, "Enjoy the tours!");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCountBefore = arrangeContext.GiftCards.Count();
        var transactionCountBefore = arrangeContext.Wallets
            .Single(wallet => wallet.Id == WalletSeed.BuyersWallet.Id).Transactions.Count;
        var expectedAvailableBalance = WalletSeed.BuyersWallet.AvailableBalance - 120m;

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(giftCardCountBefore + 1);
        var storedGiftCard = assertContext.GiftCards.Single(giftCard =>
            giftCard.CreatorId == DemoUsers.SecondExplorer && giftCard.RecipientId == DemoUsers.ThirdExplorer
            && giftCard.Message == "Enjoy the tours!");
        storedGiftCard.Amount.Should().Be(120m);
        storedGiftCard.Message.Should().Be("Enjoy the tours!");
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.BuyersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(expectedAvailableBalance);
        storedWallet.Transactions.Should().HaveCount(transactionCountBefore + 1);
        storedWallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.GiftCardPurchase
            && transaction.Amount == 120m
            && transaction.BalanceAfter == expectedAvailableBalance);
    }

    [Fact]
    public async Task Gift_card_for_a_recipient_who_does_not_exist_is_not_created()
    {
        var client = Factory.CreateClientFor(DemoUsers.SecondExplorer, "explorer");
        var request = new PurchaseGiftCardDto(Guid.NewGuid(), 120m, "Enjoy the tours!");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCountBefore = arrangeContext.GiftCards.Count();

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(giftCardCountBefore);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.BuyersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(WalletSeed.BuyersWallet.AvailableBalance);
    }

    [Fact]
    public async Task Gift_card_above_the_available_balance_is_not_created()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        var request = new PurchaseGiftCardDto(DemoUsers.ThirdExplorer, 120m, "Enjoy the tours!");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCountBefore = arrangeContext.GiftCards.Count();

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(giftCardCountBefore);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ProductOwnersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(WalletSeed.ProductOwnersWallet.AvailableBalance);
        storedWallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Gift_card_for_the_buyer_themselves_is_not_created_and_their_wallet_is_not_charged()
    {
        var client = Factory.CreateClientFor(DemoUsers.SecondExplorer, "explorer");
        var request = new PurchaseGiftCardDto(DemoUsers.SecondExplorer, 120m, "Enjoy the tours!");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCountBefore = arrangeContext.GiftCards.Count();

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(giftCardCountBefore);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.BuyersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(WalletSeed.BuyersWallet.AvailableBalance);
    }

    [Fact]
    public async Task Customer_without_a_wallet_cannot_buy_a_gift_card()
    {
        var client = Factory.CreateClientFor(DemoUsers.ThirdExplorer, "explorer");
        var request = new PurchaseGiftCardDto(DemoUsers.Explorer, 120m, "Enjoy the tours!");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCountBefore = arrangeContext.GiftCards.Count();

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(giftCardCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_buy_a_gift_card()
    {
        var client = Factory.CreateClient();
        var request = new PurchaseGiftCardDto(DemoUsers.ThirdExplorer, 120m, "Enjoy the tours!");

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_buy_a_gift_card()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new PurchaseGiftCardDto(DemoUsers.ThirdExplorer, 120m, "Enjoy the tours!");

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
