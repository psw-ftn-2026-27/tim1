using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Games.Application.QuizManagement;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizManagement;

public sealed class QuizManagementCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/games/quizzes";

    public QuizManagementCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_creates_a_draft_quiz_and_can_read_its_content()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new SaveQuizDto("Town walk", "Visit the park.", 0,
            [new QuizQuestionInputDto(null, "Which sign?", [new QuizAnswerDto("First", true), new QuizAnswerDto("Second", false)])],
            45.25, 19.85);

        var response = await client.PostAsJsonAsync(Url, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var id = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        var stored = await client.GetFromJsonAsync<QuizDto>($"{Url}/{id}", JsonOptions);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be(request.Name);
        stored.Instructions.Should().Be(request.Instructions);
        stored.ExperiencePoints.Should().Be(0);
        stored.Status.Should().Be(QuizStatus.Draft);
        stored.Latitude.Should().Be(45.25);
        stored.Longitude.Should().Be(19.85);
        stored.Questions.Should().ContainSingle().Which.Answers.Should().Equal(request.Questions![0].Answers!);
    }

    [Theory]
    [InlineData(46, 20)]
    [InlineData(45, 19)]
    public async Task Saving_a_draft_stores_the_submitted_location(double latitude, double longitude)
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = SeedQuizWithLocation();
        var url = $"{Url}/{quiz.Id}";
        var request = new SaveQuizDto("Changed", quiz.Instructions, quiz.ExperiencePoints, [], latitude, longitude);

        var response = await client.PutAsJsonAsync(url, request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var stored = await client.GetFromJsonAsync<QuizDto>(url, JsonOptions);
        stored!.Name.Should().Be("Changed");
        stored.Latitude.Should().Be(latitude);
        stored.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData("{\"name\":\"City walk\",\"instructions\":\"Read the signs.\",\"experiencePoints\":10,\"latitude\":null,\"longitude\":null}")]
    [InlineData("{\"name\":\"City walk\",\"instructions\":\"Read the signs.\",\"experiencePoints\":10}")]
    public async Task A_draft_location_can_be_removed(string json)
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var url = $"{Url}/{SeedQuizWithLocation().Id}";
        using var request = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PutAsync(url, request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var stored = await client.GetFromJsonAsync<QuizDto>(url, JsonOptions);
        stored!.Latitude.Should().BeNull();
        stored.Longitude.Should().BeNull();
    }

    [Fact]
    public async Task An_invalid_location_rejects_the_entire_quiz_update()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var url = $"{Url}/{SeedQuizWithLocation().Id}";
        var original = await client.GetFromJsonAsync<QuizDto>(url, JsonOptions);
        var request = new SaveQuizDto("Changed", "New instructions.", 0, [], 46, null);

        var response = await client.PutAsJsonAsync(url, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var stored = await client.GetFromJsonAsync<QuizDto>(url, JsonOptions);
        stored.Should().BeEquivalentTo(original);
    }

    [Fact]
    public async Task Administrator_changes_the_content_of_a_draft_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = QuizSeed.DraftCityWalk;
        var retainedQuestion = quiz.Questions[0];
        var request = new SaveQuizDto("Park walk", "Read the plaques.", 5,
            [new QuizQuestionInputDto(null, "New question", []),
             new QuizQuestionInputDto(retainedQuestion.Id, "Updated question", [new QuizAnswerDto("Changed", false)])]);

        var response = await client.PutAsJsonAsync($"{Url}/{quiz.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var stored = await client.GetFromJsonAsync<QuizDto>($"{Url}/{quiz.Id}", JsonOptions);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be(request.Name);
        stored.Instructions.Should().Be(request.Instructions);
        stored.ExperiencePoints.Should().Be(5);
        stored.Questions.Select(question => question.Text).Should().Equal("New question", "Updated question");
        stored.Questions[1].Id.Should().Be(retainedQuestion.Id);
        stored.Questions[1].Answers.Should().Equal(new QuizAnswerDto("Changed", false));
    }

    [Fact]
    public async Task A_published_quiz_rejects_changes_and_keeps_its_saved_content()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var url = $"{Url}/{QuizSeed.PublishedSquare.Id}";
        var original = await client.GetFromJsonAsync<QuizDto>(url, JsonOptions);
        var request = new SaveQuizDto("Changed", "Other instructions.", 5, []);

        var response = await client.PutAsJsonAsync(url, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var stored = await client.GetFromJsonAsync<QuizDto>(url, JsonOptions);
        stored.Should().BeEquivalentTo(original);
    }

    [Theory]
    [InlineData("{\"name\":\"Quiz\",\"instructions\":\"Read the sign.\"}")]
    [InlineData("{\"name\":\"Quiz\",\"instructions\":\"Read the sign.\",\"experiencePoints\":1.5}")]
    public async Task A_quiz_requires_whole_experience_points_in_the_request(string json)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var request = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(Url, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_an_administrator_can_create_a_quiz(bool signedIn, HttpStatusCode expectedStatus)
    {
        var client = signedIn ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer") : Factory.CreateClient();
        var request = new SaveQuizDto("Town walk", "Visit the park.", 0, []);

        var response = await client.PostAsJsonAsync(Url, request);

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_an_administrator_can_change_a_quiz(bool signedIn, HttpStatusCode expectedStatus)
    {
        var client = signedIn ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer") : Factory.CreateClient();
        var request = new SaveQuizDto("Town walk", "Visit the park.", 0, []);

        var response = await client.PutAsJsonAsync($"{Url}/{QuizSeed.DraftCityWalk.Id}", request);

        response.StatusCode.Should().Be(expectedStatus);
    }

    private Quiz SeedQuizWithLocation()
    {
        var quiz = new Quiz("City walk", "Read the signs.", 10, [], 45, 19);
        Factory.Reseed<GamesDbContext>([quiz]);
        return quiz;
    }
}
