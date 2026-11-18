using System.Net;
using System.Net.Http.Json;
using Exploration.Domain.Images;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeypointImageUpload;

public class KeypointImageUploadCommandTests : BaseIntegrationTest
{
    public KeypointImageUploadCommandTests(ExplorationApiFactory factory) : base(factory) { }

    private const string ImageUploadUrl = "/api/exploration/tours/keypoint-images";

    private static MultipartFormDataContent ImageForm(byte[] content)
    {
        var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(content), "file", "photo.png");
        return form;
    }

    [Fact]
    public async Task Explorer_uploads_a_keypoint_image_and_it_is_stored_as_unattached_with_its_file()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = ImageForm(TestImages.Png(300, 200));

        var response = await client.PostAsync(ImageUploadUrl, form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var imageId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Images.Single(image => image.Id == imageId);
        stored.UploaderId.Should().Be(WellKnownUsers.Explorer);
        stored.Status.Should().Be(ImageStatus.Unattached);
        File.Exists(Factory.StoredImagePath(stored.StoredFileName)).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_keypoint_image_leaves_neither_a_record_nor_a_file()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = ImageForm("not an image"u8.ToArray());
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var imageCountBefore = arrangeContext.Images.Count();
        var fileCountBefore = Factory.StoredImageFileCount();

        var response = await client.PostAsync(ImageUploadUrl, form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Images.Count().Should().Be(imageCountBefore);
        Factory.StoredImageFileCount().Should().Be(fileCountBefore);
    }

    [Fact]
    public async Task Expired_unattached_images_are_deleted_with_their_files_on_the_next_upload()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = ImageForm(TestImages.Png(300, 200));

        var response = await client.PostAsync(ImageUploadUrl, form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Images.Any(image => image.Id == ImageSeed.ExpiredUnattachedImage.Id).Should().BeFalse();
        File.Exists(Factory.StoredImagePath(ImageSeed.ExpiredUnattachedImage.StoredFileName)).Should().BeFalse();
        assertContext.Images.Any(image => image.Id == ImageSeed.ExplorersUnattachedImage.Id).Should().BeTrue();
        assertContext.Images.Any(image => image.Id == ImageSeed.DraftFirstKeypointPhoto.Id).Should().BeTrue();
    }

    [Fact]
    public async Task Anonymous_user_cannot_upload_a_keypoint_image()
    {
        var client = Factory.CreateClient();
        using var form = ImageForm(TestImages.Png(300, 200));

        var response = await client.PostAsync(ImageUploadUrl, form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_upload_a_keypoint_image()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = ImageForm(TestImages.Png(300, 200));

        var response = await client.PostAsync(ImageUploadUrl, form);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
