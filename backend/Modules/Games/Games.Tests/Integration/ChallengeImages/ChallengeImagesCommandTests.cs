using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Infrastructure.Images;
using Games.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Images;
using Shared.Tests;
using SkiaSharp;
using Xunit;

namespace Games.Tests.Integration.ChallengeImages;

public sealed class ChallengeImagesCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/games/images";

    public ChallengeImagesCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_uploads_a_challenge_photo_that_is_stored_with_its_file()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = PhotoForm(Png(300, 200));
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var imageCountBefore = arrangeContext.Images.Count();

        var response = await client.PostAsync(Url, form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var imageId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Images.Count().Should().Be(imageCountBefore + 1);
        var stored = assertContext.Images.Single(image => image.Id == imageId);
        File.Exists(StoredImagePath(stored.StoredFileName)).Should().BeTrue();
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_an_administrator_can_upload_a_challenge_photo(bool signedIn, HttpStatusCode expectedStatus)
    {
        var client = signedIn ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer") : Factory.CreateClient();
        using var form = PhotoForm();

        var response = await client.PostAsync(Url, form);

        response.StatusCode.Should().Be(expectedStatus);
    }

    // The request is rejected before the file is read, so its content does not matter.
    private static MultipartFormDataContent PhotoForm() => PhotoForm("photo content"u8.ToArray());

    private static MultipartFormDataContent PhotoForm(byte[] content)
    {
        var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(content), "file", "photo.png");
        return form;
    }

    private static byte[] Png(int width, int height)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque));
        bitmap.Erase(SKColors.SteelBlue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
    }

    // Reads the image root the running app was configured with, so the test checks the
    // folder the storage actually writes to.
    private string StoredImagePath(string storedFileName)
    {
        var rootFolder = Factory.Services.GetRequiredService<IConfiguration>()[LocalImageStorage.RootFolderKey]!;
        return Path.Combine(rootFolder, ImageStorageAdapter.ModuleFolder, storedFileName);
    }
}
