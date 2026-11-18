using System.Text.Json;
using System.Text.Json.Serialization;
using Games.Domain.Images;
using Games.Infrastructure.Images;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Microsoft.AspNetCore.Hosting;
using Shared.Infrastructure.Images;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration;

/// <summary>
/// Besides the test database, Games' tests get their own image folder, so they never
/// touch a developer's real images. Like the database, it is reset before every test:
/// each seeded image gets a file with <see cref="SeededImageContent"/>.
/// </summary>
public sealed class GamesApiFactory : ExplorerApiFactory
{
    // Storage returns the stored bytes unchanged, so the seeded files need not be real images.
    public static readonly byte[] SeededImageContent = "seeded image content"u8.ToArray();

    private static readonly string ImagesRootFolder = Path.Combine(Path.GetTempPath(), "explorer-test-images-games");
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
            File.WriteAllBytes(Path.Combine(ImagesFolder, image.StoredFileName), SeededImageContent);
        }
    }
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<GamesApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly GamesApiFactory Factory;

    protected BaseIntegrationTest(GamesApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<GamesDbContext>(GamesSeed.All);
        Factory.ReseedImageFiles(ImageSeed.All);
    }
}
