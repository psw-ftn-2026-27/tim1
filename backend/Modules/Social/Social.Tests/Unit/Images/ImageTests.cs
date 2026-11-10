using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Images;
using Xunit;

namespace Social.Tests.Unit.Images;

public class ImageTests
{
    private static Image CreateImage() => new(WellKnownUsers.Explorer, 800, 600, 1000);

    [Fact]
    public void Uploader_attaches_their_own_image()
    {
        var image = CreateImage();

        image.AttachToBlog(WellKnownUsers.Explorer);

        image.Status.Should().Be(ImageStatus.Attached);
        image.AttachedAt.Should().NotBeNull();
    }

    [Fact]
    public void User_cannot_attach_an_image_uploaded_by_someone_else()
    {
        var image = CreateImage();

        var attaching = () => image.AttachToBlog(WellKnownUsers.SecondExplorer);

        attaching.Should().Throw<DomainException>();
    }

    [Fact]
    public void Attached_image_cannot_be_attached_again()
    {
        var image = CreateImage();
        image.AttachToBlog(WellKnownUsers.Explorer);

        var attaching = () => image.AttachToBlog(WellKnownUsers.Explorer);

        attaching.Should().Throw<DomainException>();
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
        image.AttachToBlog(WellKnownUsers.Explorer);

        var isExpired = image.IsExpired(image.CreatedAt.AddDays(30));

        isExpired.Should().BeFalse();
    }
}
