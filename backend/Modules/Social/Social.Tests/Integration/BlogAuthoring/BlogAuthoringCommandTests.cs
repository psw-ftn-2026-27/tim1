using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Infrastructure.Images;
using Shared.Tests;
using Social.Application.BlogAuthoring;
using Social.Domain.Blogs;
using Social.Domain.Images;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogAuthoring;

public class BlogAuthoringCommandTests : BaseIntegrationTest
{
    public BlogAuthoringCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_creates_a_draft_blog_with_images()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var imageId = ImageSeed.ExplorersUnattachedImage.Id;
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", [imageId]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore + 1);
        var stored = assertContext.Blogs.Single(blog => blog.Id == createdId);
        stored.AuthorId.Should().Be(WellKnownUsers.Explorer);
        stored.Status.Should().Be(BlogStatus.Draft);
        stored.Images.Should().Equal(imageId);
        stored.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task Explorer_cannot_create_a_blog_without_a_title()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateBlogDto("   ", "Opis novog bloga.", []);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_blog()
    {
        var client = Factory.CreateClient();
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", []);

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", []);

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Draft_blog_is_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public async Task Explorer_cannot_publish_another_authors_blog()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Draft);
    }

    [Fact]
    public async Task Explorer_creates_a_blog_with_their_own_images_and_the_images_become_attached()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var imageId = ImageSeed.ExplorersUnattachedImage.Id;
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", [imageId]);

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Single(blog => blog.Id == createdId).Images.Should().Equal(imageId);
        assertContext.Images.Single(image => image.Id == imageId).Status.Should().Be(ImageStatus.Attached);
    }

    [Fact]
    public async Task Blog_is_not_saved_when_one_of_its_images_cannot_be_attached()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.",
            [ImageSeed.ExplorersUnattachedImage.Id, ImageSeed.SecondExplorersUnattachedImage.Id]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore);
        assertContext.Images.Single(image => image.Id == ImageSeed.ExplorersUnattachedImage.Id)
            .Status.Should().Be(ImageStatus.Unattached);
    }

    [Fact]
    public async Task Blog_with_an_image_that_does_not_exist_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", [Guid.NewGuid()]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore);
    }

    [Fact]
    public async Task Explorer_uploads_an_image_and_it_is_stored_as_unattached_with_its_file()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = ImageForm(TestImages.Png(300, 200));

        var response = await client.PostAsync("/api/social/blogs/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var imageId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Images.Single(image => image.Id == imageId);
        stored.UploaderId.Should().Be(WellKnownUsers.Explorer);
        stored.Status.Should().Be(ImageStatus.Unattached);
        stored.Width.Should().Be(300);
        stored.Height.Should().Be(200);
        File.Exists(Factory.StoredImagePath(stored.StoredFileName)).Should().BeTrue();
    }

    [Fact]
    public async Task File_larger_than_5_megabytes_is_rejected_before_processing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = ImageForm(new byte[ImageProcessor.MaxFileBytes + 1024 * 1024]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var imageCountBefore = arrangeContext.Images.Count();

        var response = await client.PostAsync("/api/social/blogs/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Images.Count().Should().Be(imageCountBefore);
    }

    [Fact]
    public async Task Invalid_image_leaves_neither_a_record_nor_a_file()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = ImageForm("not an image"u8.ToArray());
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var imageCountBefore = arrangeContext.Images.Count();
        var fileCountBefore = Factory.StoredImageFileCount();

        var response = await client.PostAsync("/api/social/blogs/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Images.Count().Should().Be(imageCountBefore);
        Factory.StoredImageFileCount().Should().Be(fileCountBefore);
    }

    [Fact]
    public async Task Expired_unattached_images_are_deleted_with_their_files_on_the_next_upload()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = ImageForm(TestImages.Png(300, 200));

        var response = await client.PostAsync("/api/social/blogs/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Images.Any(image => image.Id == ImageSeed.ExpiredUnattachedImage.Id).Should().BeFalse();
        File.Exists(Factory.StoredImagePath(ImageSeed.ExpiredUnattachedImage.StoredFileName)).Should().BeFalse();
        assertContext.Images.Any(image => image.Id == ImageSeed.OldFortressPhotoOnDraft.Id).Should().BeTrue();
        File.Exists(Factory.StoredImagePath(ImageSeed.OldFortressPhotoOnDraft.StoredFileName)).Should().BeTrue();
        assertContext.Images.Any(image => image.Id == ImageSeed.ExplorersUnattachedImage.Id).Should().BeTrue();
        File.Exists(Factory.StoredImagePath(ImageSeed.ExplorersUnattachedImage.StoredFileName)).Should().BeTrue();
    }

    [Fact]
    public async Task Anonymous_user_cannot_upload_an_image()
    {
        var client = Factory.CreateClient();
        using var form = ImageForm(TestImages.Png(300, 200));

        var response = await client.PostAsync("/api/social/blogs/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_upload_an_image()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = ImageForm(TestImages.Png(300, 200));

        var response = await client.PostAsync("/api/social/blogs/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_updates_the_content_of_their_blog_and_a_new_image_becomes_attached()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var keptImageId = ImageSeed.OldFortressPhotoOnDraft.Id;
        var newImageId = ImageSeed.ExplorersUnattachedImage.Id;
        var request = new UpdateBlogDto("Novi naslov", "Novi opis obilaska tvrđave.", [keptImageId, newImageId]);

        var response = await client.PutAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Title.Should().Be("Novi naslov");
        stored.Description.Should().Be("Novi opis obilaska tvrđave.");
        stored.Images.Should().Equal(keptImageId, newImageId);
        assertContext.Images.Single(image => image.Id == newImageId).Status.Should().Be(ImageStatus.Attached);
    }

    [Fact]
    public async Task Explorer_cannot_update_another_authors_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new UpdateBlogDto("Novi naslov", "Novi opis obilaska tvrđave.", []);

        var response = await client.PutAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Title.Should().Be(BlogSeed.FortressImpressions.Title);
        stored.Images.Should().Equal(BlogSeed.FortressImpressions.Images);
    }

    [Fact]
    public async Task Blog_cannot_be_updated_without_a_title()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateBlogDto("   ", "Novi opis obilaska tvrđave.", []);

        var response = await client.PutAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Title.Should().Be(BlogSeed.FortressImpressions.Title);
        stored.Images.Should().Equal(BlogSeed.FortressImpressions.Images);
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_blog()
    {
        var client = Factory.CreateClient();
        var request = new UpdateBlogDto("Novi naslov", "Novi opis obilaska tvrđave.", []);

        var response = await client.PutAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_update_a_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateBlogDto("Novi naslov", "Novi opis obilaska tvrđave.", []);

        var response = await client.PutAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static MultipartFormDataContent ImageForm(byte[] content)
    {
        var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(content), "file", "photo.png");
        return form;
    }
}
