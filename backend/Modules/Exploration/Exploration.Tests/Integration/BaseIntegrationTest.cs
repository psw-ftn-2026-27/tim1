using System.Text.Json;
using System.Text.Json.Serialization;
using Exploration.Domain.Images;
using Exploration.Infrastructure.Images;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using Microsoft.AspNetCore.Hosting;
using Shared.Infrastructure.Images;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration;

/// <summary>
/// Besides the test database, Exploration's tests get their own image folder, so they never
/// touch a developer's real images. Like the database, it is reset before every test:
/// each seeded image gets a file with <see cref="SeededImageContent"/>.
/// </summary>
public sealed class ExplorationApiFactory : ExplorerApiFactory
{
    public static readonly byte[] SeededImageContent = TestImages.Webp(100, 100);

    private static readonly string ImagesRootFolder = Path.Combine(Path.GetTempPath(), "explorer-test-images-exploration");
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
public sealed class IntegrationCollection : ICollectionFixture<ExplorationApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly ExplorationApiFactory Factory;

    protected BaseIntegrationTest(ExplorationApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<ExplorationDbContext>(ExplorationSeed.All);
        Factory.ReseedImageFiles(ImageSeed.All);
    }
}
