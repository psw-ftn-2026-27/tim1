using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Payment.Tests.Integration.Seeds;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.PurchaseVerification;

public class PurchaseVerificationQueryTests : BaseIntegrationTest
{
    public PurchaseVerificationQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_of_a_tour_owns_it()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var owns = await paymentApi.HasPurchasedAsync(
            WellKnownUsers.SecondExplorer,
            ProductType.Tour,
            PurchaseSeed.SecondExplorersRide.Reference.EntityId);

        owns.Should().BeTrue();
    }

    [Fact]
    public async Task Tour_nobody_bought_is_owned_by_no_one()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var owns = await paymentApi.HasPurchasedAsync(
            WellKnownUsers.SecondExplorer,
            ProductType.Tour,
            ProductSeed.DemoExplorersTour.Reference.EntityId);

        owns.Should().BeFalse();
    }

    [Fact]
    public async Task Tour_another_user_bought_is_not_owned_by_this_one()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var owns = await paymentApi.HasPurchasedAsync(
            WellKnownUsers.Explorer,
            ProductType.Tour,
            PurchaseSeed.SecondExplorersRide.Reference.EntityId);

        owns.Should().BeFalse();
    }

    [Fact]
    public async Task Kind_of_content_the_platform_does_not_sell_cannot_be_owned()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var asking = async () => await paymentApi.HasPurchasedAsync(
            WellKnownUsers.SecondExplorer,
            (ProductType)99,
            PurchaseSeed.SecondExplorersRide.Reference.EntityId);

        await asking.Should().ThrowAsync<DomainException>();
    }
}
