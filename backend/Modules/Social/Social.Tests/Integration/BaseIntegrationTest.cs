using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Shared.Infrastructure.Images;
using Shared.Tests;
using Social.Domain.Images;
using Social.Infrastructure.Images;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration;

/// <summary>
/// Besides the test database, Social's tests get their own image folder, so they never
/// touch a developer's real images. Like the database, it is reset before every test:
/// each seeded image gets a file with <see cref="SeededImageContent"/>.
/// </summary>
public sealed class SocialApiFactory : ExplorerApiFactory
{
    public static readonly byte[] SeededImageContent = TestImages.Webp(100, 100);

    private static readonly string ImagesRootFolder = Path.Combine(Path.GetTempPath(), "explorer-test-images-social");
    private static readonly string ImagesFolder = Path.Combine(ImagesRootFolder, ImageStorageAdapter.ModuleFolder);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting(LocalImageStorage.RootFolderKey, ImagesRootFolder);
    }

    public void ReseedImageFiles(IEnumerable<Image> images)
    {
        if (Directory.Exists(ImagesFolder))
        {
            Directory.Delete(ImagesFolder, recursive: true);
        }
        Directory.CreateDirectory(ImagesFolder);

        foreach (var image in images)
        {
            File.WriteAllBytes(StoredImagePath(image.StoredFileName), SeededImageContent);
        }
    }

    public string StoredImagePath(string storedFileName) => Path.Combine(ImagesFolder, storedFileName);

    public int StoredImageFileCount() => Directory.GetFiles(ImagesFolder).Length;
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<SocialApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly SocialApiFactory Factory;

    protected BaseIntegrationTest(SocialApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<SocialDbContext>(SocialSeed.All);
        Factory.ReseedImageFiles(ImageSeed.All);
    }
}
