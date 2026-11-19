using Exploration.Domain.Images;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.Images;

public class ImageTests
{
    private static Image CreateImage() => new(WellKnownUsers.Explorer, 800, 600, 1000);

    [Fact]
    public void Uploader_attaches_their_own_image()
    {
        var image = CreateImage();

        image.AttachToKeypoint(WellKnownUsers.Explorer);

        image.Status.Should().Be(ImageStatus.Attached);
        image.AttachedAt.Should().NotBeNull();
    }

    [Fact]
    public void User_cannot_attach_an_image_uploaded_by_someone_else()
    {
        var image = CreateImage();

        var attaching = () => image.AttachToKeypoint(WellKnownUsers.SecondExplorer);

        attaching.Should().Throw<DomainException>();
    }

    [Fact]
    public void Attached_image_cannot_be_attached_again()
    {
        var image = CreateImage();
        image.AttachToKeypoint(WellKnownUsers.Explorer);

        var attaching = () => image.AttachToKeypoint(WellKnownUsers.Explorer);

        attaching.Should().Throw<DomainException>();
    }

    [Fact]
    public void Attached_image_can_be_detached_and_then_attached_again()
    {
        var image = CreateImage();
        image.AttachToKeypoint(WellKnownUsers.Explorer);

        image.Detach();

        image.Status.Should().Be(ImageStatus.Unattached);
        image.AttachedAt.Should().BeNull();
    }

    [Fact]
    public void Unattached_image_cannot_be_detached()
    {
        var image = CreateImage();

        var detaching = () => image.Detach();

        detaching.Should().Throw<DomainException>();
    }

    [Fact]
    public void Detached_image_expires_at_once()
    {
        var image = new Image(WellKnownUsers.Explorer, 800, 600, 1000, DateTime.UtcNow.AddDays(-10));
        image.AttachToKeypoint(WellKnownUsers.Explorer);

        image.Detach();

        image.IsExpired(DateTime.UtcNow).Should().BeTrue();
    }

    [Theory]
    [InlineData(23, false)]
    [InlineData(25, true)]
    public void Unattached_image_expires_after_24_hours(int hoursLater, bool expired)
    {
        var image = CreateImage();

        var isExpired = image.IsExpired(image.CreatedAt.AddHours(hoursLater));

        isExpired.Should().Be(expired);
    }

    [Fact]
    public void Attached_image_never_expires()
    {
        var image = CreateImage();
        image.AttachToKeypoint(WellKnownUsers.Explorer);

        var isExpired = image.IsExpired(image.CreatedAt.AddDays(30));

        isExpired.Should().BeFalse();
    }
}
