using System.Net;
using FluentAssertions;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Checkout;

public class CheckoutCommandTests : BaseIntegrationTest
{
    public CheckoutCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Bought_cart_pays_every_owner_leaves_the_purchases_behind_and_stays_empty()
    {
        var client = Factory.CreateClientFor(DemoUsers.SecondExplorer, "explorer");
        var total = ShoppingCartSeed.BuyersCart.Total();

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var buyersWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.BuyersWallet.Id);
        buyersWallet.AvailableBalance.Should().Be(WalletSeed.BuyersWallet.AvailableBalance - total);
        var ownersWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ProductOwnersWallet.Id);
        ownersWallet.AvailableBalance.Should().Be(
            WalletSeed.ProductOwnersWallet.AvailableBalance + ProductSeed.DemoExplorersTour.Price);
        var forgottenOwnersWallet = assertContext.Wallets.Single(wallet =>
            wallet.OwnerId == ProductSeed.TourOfAForgottenOwner.OwnerId);
        forgottenOwnersWallet.AvailableBalance.Should().Be(ProductSeed.TourOfAForgottenOwner.Price);
        var purchases = assertContext.Purchases
            .Where(purchase => purchase.BuyerId == DemoUsers.SecondExplorer)
            .ToList();
        purchases.Should().HaveCount(ShoppingCartSeed.BuyersCart.Items.Count);
        purchases.Should().ContainSingle(purchase =>
            purchase.Reference == ProductSeed.DemoExplorersTour.Reference
            && purchase.Price == ProductSeed.DemoExplorersTour.Price);
        purchases.Should().ContainSingle(purchase =>
            purchase.Reference == ProductSeed.TourOfAForgottenOwner.Reference
            && purchase.Price == ProductSeed.TourOfAForgottenOwner.Price);
        assertContext.ShoppingCarts.Single(cart => cart.OwnerId == DemoUsers.SecondExplorer)
            .Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Buyer_gets_one_payment_transaction_for_each_bought_item_naming_the_product()
    {
        var client = Factory.CreateClientFor(DemoUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var buyersWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.BuyersWallet.Id);
        var payments = buyersWallet.Transactions
            .Where(transaction => transaction.Type == TransactionType.Purchase)
            .ToList();
        payments.Should().HaveCount(ShoppingCartSeed.BuyersCart.Items.Count);
        payments.Should().ContainSingle(payment =>
            payment.ProductName == ProductSeed.DemoExplorersTour.Name
            && payment.Amount == ProductSeed.DemoExplorersTour.Price);
        payments.Should().ContainSingle(payment =>
            payment.ProductName == ProductSeed.TourOfAForgottenOwner.Name
            && payment.Amount == ProductSeed.TourOfAForgottenOwner.Price);
    }

    [Fact]
    public async Task Customer_whose_balance_is_below_the_total_of_their_cart_buys_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var purchaseCountBefore = arrangeContext.Purchases.Count();

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Purchases.Count().Should().Be(purchaseCountBefore);
        var customersWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ExplorersWallet.Id);
        customersWallet.AvailableBalance.Should().Be(WalletSeed.ExplorersWallet.AvailableBalance);
        customersWallet.Transactions.Should().BeEmpty();
        var ownersWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ProductOwnersWallet.Id);
        ownersWallet.AvailableBalance.Should().Be(WalletSeed.ProductOwnersWallet.AvailableBalance);
        ownersWallet.Transactions.Should().BeEmpty();
        assertContext.ShoppingCarts.Single(cart => cart.OwnerId == WellKnownUsers.Explorer)
            .Items.Should().HaveCount(ShoppingCartSeed.ExplorersCart.Items.Count);
    }

    [Fact]
    public async Task Cart_holding_a_product_the_customer_already_bought_is_not_bought()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var purchaseCountBefore = arrangeContext.Purchases.Count();

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Purchases.Count().Should().Be(purchaseCountBefore);
        var customersWallet = assertContext.Wallets.Single(wallet =>
            wallet.Id == WalletSeed.SecondExplorersWallet.Id);
        customersWallet.AvailableBalance.Should().Be(WalletSeed.SecondExplorersWallet.AvailableBalance);
        customersWallet.Transactions.Should().HaveCount(WalletSeed.SecondExplorersWallet.Transactions.Count);
        assertContext.ShoppingCarts.Single(cart => cart.OwnerId == WellKnownUsers.SecondExplorer)
            .Items.Should().HaveCount(ShoppingCartSeed.SecondExplorersCart.Items.Count);
    }

    [Fact]
    public async Task Cart_holding_a_product_that_is_not_registered_is_not_bought()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var purchaseCountBefore = arrangeContext.Purchases.Count();

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Purchases.Count().Should().Be(purchaseCountBefore);
        var customersWallet = assertContext.Wallets.Single(wallet =>
            wallet.Id == WalletSeed.ProductOwnersWallet.Id);
        customersWallet.AvailableBalance.Should().Be(WalletSeed.ProductOwnersWallet.AvailableBalance);
        assertContext.ShoppingCarts.Single(cart => cart.OwnerId == DemoUsers.Explorer)
            .Items.Should().HaveCount(ShoppingCartSeed.CartWithAnUnregisteredProduct.Items.Count);
    }

    [Fact]
    public async Task Anonymous_user_cannot_buy_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_buy_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
