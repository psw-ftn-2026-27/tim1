using FluentAssertions;
using Payment.Domain.Products;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Purchases;

public class CartCheckoutTests
{
    private static readonly ProductReference FortressTour =
        new(ProductType.Tour, Guid.Parse("40000000-0000-0000-0000-000000000001"));

    private static readonly ProductReference MountainRide =
        new(ProductType.Tour, Guid.Parse("40000000-0000-0000-0000-000000000002"));

    private const string FortressTourName = "Walk around the Petrovaradin fortress";
    private const string MountainRideName = "Ride across Fruska gora";

    private static readonly Guid Buyer = WellKnownUsers.Explorer;
    private static readonly Guid FortressTourOwner = WellKnownUsers.SecondExplorer;
    private static readonly Guid MountainRideOwner = Guid.Parse("90000000-0000-0000-0000-000000000001");

    private static ShoppingCart CreateCartWithBothTours()
    {
        var cart = new ShoppingCart(Buyer);
        cart.AddItem(FortressTour, FortressTourName, 1200m);
        cart.AddItem(MountainRide, MountainRideName, 900m);
        return cart;
    }

    private static List<Product> CreateBothProducts()
    {
        return
        [
            new Product(FortressTour, FortressTourName, 1200m, FortressTourOwner),
            new Product(MountainRide, MountainRideName, 900m, MountainRideOwner)
        ];
    }

    private static Wallet CreateBuyerWallet()
    {
        var wallet = new Wallet(Buyer);
        wallet.Deposit(5000m);
        return wallet;
    }

    private static Dictionary<Guid, Wallet> CreateOwnerWallets()
    {
        return new Dictionary<Guid, Wallet>
        {
            [FortressTourOwner] = new Wallet(FortressTourOwner),
            [MountainRideOwner] = new Wallet(MountainRideOwner)
        };
    }

    [Fact]
    public void Bought_cart_takes_its_total_from_the_buyer_and_pays_every_owner_the_price_of_their_item()
    {
        var cart = CreateCartWithBothTours();
        var buyerWallet = CreateBuyerWallet();
        var ownerWallets = CreateOwnerWallets();

        new CartCheckout().Checkout(cart, CreateBothProducts(), buyerWallet, ownerWallets, []);

        buyerWallet.AvailableBalance.Should().Be(2900m);
        ownerWallets[FortressTourOwner].AvailableBalance.Should().Be(1200m);
        ownerWallets[MountainRideOwner].AvailableBalance.Should().Be(900m);
    }

    [Fact]
    public void Bought_cart_leaves_one_purchase_for_each_item_it_held()
    {
        var cart = CreateCartWithBothTours();
        var startedAt = DateTime.UtcNow;

        var purchases = new CartCheckout().Checkout(
            cart, CreateBothProducts(), CreateBuyerWallet(), CreateOwnerWallets(), []);

        purchases.Should().HaveCount(2);
        var fortressPurchase = purchases.Single(purchase => purchase.Reference == FortressTour);
        fortressPurchase.BuyerId.Should().Be(Buyer);
        fortressPurchase.Price.Should().Be(1200m);
        fortressPurchase.PurchasedAt.Should().BeOnOrAfter(startedAt);
        purchases.Should().ContainSingle(purchase =>
            purchase.Reference == MountainRide && purchase.BuyerId == Buyer && purchase.Price == 900m);
    }

    [Fact]
    public void Bought_cart_is_left_empty()
    {
        var cart = CreateCartWithBothTours();

        new CartCheckout().Checkout(cart, CreateBothProducts(), CreateBuyerWallet(), CreateOwnerWallets(), []);

        cart.Items.Should().BeEmpty();
        cart.Total().Should().Be(0);
    }

    [Fact]
    public void Cart_holding_a_product_the_buyer_already_owns_is_not_bought()
    {
        var cart = CreateCartWithBothTours();
        var buyerWallet = CreateBuyerWallet();
        var ownerWallets = CreateOwnerWallets();
        Purchase[] ownedPurchases = [new Purchase(Buyer, FortressTour, 1200m)];

        var buying = () => new CartCheckout().Checkout(
            cart, CreateBothProducts(), buyerWallet, ownerWallets, ownedPurchases);

        buying.Should().Throw<DomainException>();
        cart.Items.Should().HaveCount(2);
        buyerWallet.AvailableBalance.Should().Be(5000m);
        ownerWallets[FortressTourOwner].AvailableBalance.Should().Be(0);
        ownerWallets[MountainRideOwner].AvailableBalance.Should().Be(0);
    }

    [Fact]
    public void Empty_cart_is_not_bought()
    {
        var cart = new ShoppingCart(Buyer);
        var buyerWallet = CreateBuyerWallet();

        var buying = () => new CartCheckout().Checkout(cart, [], buyerWallet, CreateOwnerWallets(), []);

        buying.Should().Throw<DomainException>();
        buyerWallet.AvailableBalance.Should().Be(5000m);
        buyerWallet.Transactions.Should().NotContain(transaction =>
            transaction.Type == TransactionType.Purchase);
    }
}
