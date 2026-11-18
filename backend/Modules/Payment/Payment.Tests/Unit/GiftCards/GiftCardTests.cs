using FluentAssertions;
using Payment.Domain.GiftCards;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.GiftCards;

public class GiftCardTests
{
    private static GiftCard CreateGiftCard() =>
        new(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, 100m, "Happy birthday!");

    [Theory]
    [InlineData(10)]
    [InlineData(500)]
    [InlineData(1000)]
    public void Gift_card_amount_can_be_any_whole_number_from_10_to_1000(decimal amount)
    {
        var giftCard = new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, amount, "Enjoy!");

        giftCard.Amount.Should().Be(amount);
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(1001)]
    [InlineData(10.5)]
    public void Gift_card_amount_outside_the_range_or_not_whole_is_rejected(decimal amount)
    {
        var creation = () => new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, amount, "Enjoy!");

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Gift_card_message_can_have_200_characters()
    {
        var message = new string('a', 200);

        var giftCard = new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, 100m, message);

        giftCard.Message.Should().Be(message);
    }

    [Fact]
    public void Gift_card_message_longer_than_200_characters_is_rejected()
    {
        var message = new string('a', 201);

        var creation = () => new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, 100m, message);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_cannot_create_a_gift_card_for_themselves()
    {
        var creation = () => new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.Explorer, 100m, "Enjoy!");

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Gift_card_is_valid_until_three_years_after_its_creation()
    {
        var giftCard = CreateGiftCard();

        var status = giftCard.StatusAt(giftCard.CreatedAt.AddYears(3));

        status.Should().Be(GiftCardStatus.Valid);
    }

    [Fact]
    public void Unused_gift_card_is_expired_after_three_years_from_its_creation()
    {
        var giftCard = CreateGiftCard();

        var status = giftCard.StatusAt(giftCard.CreatedAt.AddYears(3).AddDays(1));

        status.Should().Be(GiftCardStatus.Expired);
    }
}
