using System.Net;
using System.Net.Http.Json;
using Exploration.Application.KeypointAuthoring;
using Exploration.Domain.Images;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeypointAuthoring;

public class KeypointAuthoringCommandTests : BaseIntegrationTest
{
    private static readonly Guid DraftId = TourSeed.DraftWithKeypoints.Id;
    private static readonly Guid DraftKeypointId = TourSeed.DraftWithKeypoints.Keypoints[0].Id;
    private static readonly Guid DraftKeypointImageId = TourSeed.DraftWithKeypoints.Keypoints[0].ImageId;
    private static readonly Guid PublishedId =TourSeed.PublishedVineyards.Id;
    private static readonly Guid PublishedKeypointId = TourSeed.PublishedVineyards.Keypoints[0].Id;
    private static readonly Guid UnattachedImageId = ImageSeed.ExplorersUnattachedImage.Id;

    public KeypointAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    private static AddKeypointDto NewKeypoint(Guid imageId) =>
        new("Nova tačka", "Opis nove tačke.", imageId, "Tajna", new CoordinateDto(45.3, 19.9), 7.5);

    private static UpdateKeypointDto Update(Guid imageId) =>
        new("Izmenjena tačka", "Izmenjen opis.", imageId, null, new CoordinateDto(45.4, 19.95), 6.5);

    private static string LengthUrl(Guid tourId) => $"/api/exploration/tours/{tourId}/length";

    private static string KeypointsUrl(Guid tourId) => $"/api/exploration/tours/{tourId}/keypoints";

    private static string KeypointUrl(Guid tourId, Guid keypointId) => $"/api/exploration/tours/{tourId}/keypoints/{keypointId}";

    private static string VisibilityUrl(Guid tourId, Guid keypointId) => $"{KeypointUrl(tourId, keypointId)}/visibility";

    [Fact]
    public async Task Author_adds_a_keypoint_to_a_draft_tour_and_the_tour_length_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = NewKeypoint(UnattachedImageId);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keypointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Count;

        var response = await client.PostAsJsonAsync(KeypointsUrl(DraftId), request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == DraftId);
        stored.Keypoints.Should().HaveCount(keypointCountBefore + 1);
        stored.Keypoints.Should().Contain(keypoint => keypoint.Name == request.Name && !keypoint.PubliclyVisible);
        stored.Length.Should().Be(request.LengthKm);
        assertContext.Images.Single(image => image.Id == UnattachedImageId).Status.Should().Be(ImageStatus.Attached);
    }

    [Fact]
    public async Task Author_updates_a_keypoint_of_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = Update(UnattachedImageId);

        var response = await client.PutAsJsonAsync(KeypointUrl(DraftId, DraftKeypointId), request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Single(keypoint => keypoint.Id == DraftKeypointId);
        stored.Name.Should().Be(request.Name);
        stored.Location.Latitude.Should().Be(request.Location.Latitude);
        stored.ImageId.Should().Be(UnattachedImageId);
        assertContext.Images.Single(image => image.Id == UnattachedImageId).Status.Should().Be(ImageStatus.Attached);
        assertContext.Images.Single(image => image.Id == DraftKeypointImageId).Status.Should().Be(ImageStatus.Unattached);
    }

    [Fact]
    public async Task Author_removes_a_keypoint_of_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(KeypointUrl(DraftId, DraftKeypointId));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Should().NotContain(keypoint => keypoint.Id == DraftKeypointId);
        assertContext.Images.Single(image => image.Id == DraftKeypointImageId).Status.Should().Be(ImageStatus.Unattached);
    }

    [Fact]
    public async Task Author_sets_the_length_of_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync(LengthUrl(DraftId), new SetTourLengthDto(1.5));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Length.Should().Be(1.5);
    }

    [Fact]
    public async Task Tour_length_is_not_set_to_a_negative_value()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync(LengthUrl(DraftId), new SetTourLengthDto(-1));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Length.Should().Be(TourSeed.DraftWithKeypoints.Length);
    }

    [Fact]
    public async Task Explorer_cannot_set_the_length_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PutAsJsonAsync(LengthUrl(DraftId), new SetTourLengthDto(1.5));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Author_changes_the_visibility_of_a_keypoint()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync(VisibilityUrl(DraftId, DraftKeypointId), new SetKeypointVisibilityDto(true));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Single(keypoint => keypoint.Id == DraftKeypointId);
        stored.PubliclyVisible.Should().BeTrue();
    }

    [Fact]
    public async Task Keypoint_is_not_added_with_an_image_uploaded_by_someone_else()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var foreignImageId = ImageSeed.SecondExplorersUnattachedImage.Id;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keypointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Count;

        var response = await client.PostAsJsonAsync(KeypointsUrl(DraftId), NewKeypoint(foreignImageId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Should().HaveCount(keypointCountBefore);
        assertContext.Images.Single(image => image.Id == foreignImageId).Status.Should().Be(ImageStatus.Unattached);
    }

    [Fact]
    public async Task Keypoint_is_not_added_with_an_image_that_is_already_attached()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var attachedImageId = ImageSeed.RiversideStartPhoto.Id;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keypointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Count;

        var response = await client.PostAsJsonAsync(KeypointsUrl(DraftId), NewKeypoint(attachedImageId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Should().HaveCount(keypointCountBefore);
    }

    [Fact]
    public async Task Keypoint_is_not_updated_with_an_image_uploaded_by_someone_else()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var foreignImageId = ImageSeed.SecondExplorersUnattachedImage.Id;

        var response = await client.PutAsJsonAsync(KeypointUrl(DraftId, DraftKeypointId), Update(foreignImageId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Single(keypoint => keypoint.Id == DraftKeypointId)
            .ImageId.Should().Be(ImageSeed.DraftFirstKeypointPhoto.Id);
        assertContext.Images.Single(image => image.Id == foreignImageId).Status.Should().Be(ImageStatus.Unattached);
    }

    [Fact]
    public async Task Keypoint_is_not_updated_with_an_image_that_is_already_attached()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var attachedImageId = ImageSeed.RiversideStartPhoto.Id;

        var response = await client.PutAsJsonAsync(KeypointUrl(DraftId, DraftKeypointId), Update(attachedImageId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Single(keypoint => keypoint.Id == DraftKeypointId)
            .ImageId.Should().Be(ImageSeed.DraftFirstKeypointPhoto.Id);
    }

    [Fact]
    public async Task Explorer_cannot_add_a_keypoint_to_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keypointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Count;

        var response = await client.PostAsJsonAsync(KeypointsUrl(DraftId), NewKeypoint(UnattachedImageId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Should().HaveCount(keypointCountBefore);
    }

    [Fact]
    public async Task Explorer_cannot_update_a_keypoint_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        var nameBefore = TourSeed.DraftWithKeypoints.Keypoints[0].Name;

        var response = await client.PutAsJsonAsync(KeypointUrl(DraftId, DraftKeypointId), Update(UnattachedImageId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Single(keypoint => keypoint.Id == DraftKeypointId).Name.Should().Be(nameBefore);
    }

    [Fact]
    public async Task Explorer_cannot_remove_a_keypoint_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.DeleteAsync(KeypointUrl(DraftId, DraftKeypointId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Should().Contain(keypoint => keypoint.Id == DraftKeypointId);
    }

    [Fact]
    public async Task Explorer_cannot_change_the_visibility_of_a_keypoint_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PutAsJsonAsync(VisibilityUrl(DraftId, DraftKeypointId), new SetKeypointVisibilityDto(true));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == DraftId).Keypoints.Single(keypoint => keypoint.Id == DraftKeypointId).PubliclyVisible.Should().BeFalse();
    }

    [Fact]
    public async Task Keypoint_is_not_added_to_a_tour_that_is_not_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keypointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == PublishedId).Keypoints.Count;

        var response = await client.PostAsJsonAsync(KeypointsUrl(PublishedId), NewKeypoint(UnattachedImageId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == PublishedId).Keypoints.Should().HaveCount(keypointCountBefore);
    }

    [Fact]
    public async Task Keypoint_is_not_updated_in_a_tour_that_is_not_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var nameBefore = TourSeed.PublishedVineyards.Keypoints[0].Name;

        var response = await client.PutAsJsonAsync(KeypointUrl(PublishedId, PublishedKeypointId), Update(UnattachedImageId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == PublishedId).Keypoints.Single(keypoint => keypoint.Id == PublishedKeypointId).Name.Should().Be(nameBefore);
    }

    [Fact]
    public async Task Keypoint_is_not_removed_from_a_tour_that_is_not_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(KeypointUrl(PublishedId, PublishedKeypointId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == PublishedId).Keypoints.Should().Contain(keypoint => keypoint.Id == PublishedKeypointId);
    }

    [Fact]
    public async Task Author_changes_the_visibility_of_a_keypoint_in_a_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync(VisibilityUrl(PublishedId, PublishedKeypointId), new SetKeypointVisibilityDto(false));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == PublishedId).Keypoints.Single(keypoint => keypoint.Id == PublishedKeypointId).PubliclyVisible.Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_keypoint()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(KeypointsUrl(DraftId), NewKeypoint(UnattachedImageId));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_keypoint()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync(KeypointUrl(DraftId, DraftKeypointId), Update(UnattachedImageId));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_keypoint()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync(KeypointUrl(DraftId, DraftKeypointId));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_the_visibility_of_a_keypoint()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync(VisibilityUrl(DraftId, DraftKeypointId), new SetKeypointVisibilityDto(true));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_keypoint()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(KeypointsUrl(DraftId), NewKeypoint(UnattachedImageId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_cannot_update_a_keypoint()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsJsonAsync(KeypointUrl(DraftId, DraftKeypointId), Update(UnattachedImageId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_keypoint()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync(KeypointUrl(DraftId, DraftKeypointId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_cannot_change_the_visibility_of_a_keypoint()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsJsonAsync(VisibilityUrl(DraftId, DraftKeypointId), new SetKeypointVisibilityDto(true));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
