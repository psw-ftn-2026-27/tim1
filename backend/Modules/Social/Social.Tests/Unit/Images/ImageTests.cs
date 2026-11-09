using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Images;
using Xunit;

namespace Social.Tests.Unit.Images;

public class ImageTests
{
    private static Image CreateImage() => new(WellKnownUsers.Explorer, new ImageDimensions(800, 600), 1000);

    [Theory]
    [InlineData(99, 600)]
    [InlineData(800, 99)]
    [InlineData(8001, 600)]
    [InlineData(800, 8001)]
    [InlineData(8000, 8000)]
    public void Image_outside_the_allowed_dimensions_cannot_exist(int width, int height)
    {
        var creation = () => new ImageDimensions(width, height);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Uploader_attaches_their_own_image()
    {
        var image = CreateImage();

        image.AttachTo(WellKnownUsers.Explorer);

        image.Status.Should().Be(ImageStatus.Attached);
        image.AttachedAt.Should().NotBeNull();
    }

    [Fact]
    public void User_cannot_attach_an_image_uploaded_by_someone_else()
    {
        var image = CreateImage();

        var attaching = () => image.AttachTo(WellKnownUsers.SecondExplorer);

        attaching.Should().Throw<DomainException>();
    }

    [Fact]
    public void Attached_image_cannot_be_attached_again()
    {
        var image = CreateImage();
        image.AttachTo(WellKnownUsers.Explorer);

        var attaching = () => image.AttachTo(WellKnownUsers.Explorer);

        attaching.Should().Throw<DomainException>();
    }

    [Fact]
    public void Unattached_image_expires_after_24_hours()
    {
        var image = CreateImage();

        image.IsExpired(image.CreatedAt.AddHours(23)).Should().BeFalse();
        image.IsExpired(image.CreatedAt.AddHours(25)).Should().BeTrue();
    }

    [Fact]
    public void Attached_image_never_expires()
    {
        var image = CreateImage();
        image.AttachTo(WellKnownUsers.Explorer);

        image.IsExpired(image.CreatedAt.AddDays(30)).Should().BeFalse();
    }
}
