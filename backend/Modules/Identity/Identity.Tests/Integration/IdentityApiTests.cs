using FluentAssertions;
using Identity.Contracts;
using Identity.Core;
using Identity.Tests.Integration.Seeds;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Tests.Integration;

public sealed class IdentityApiTests : BaseIntegrationTest
{
    public IdentityApiTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Search_without_text_returns_the_first_page_of_all_users_and_their_total()
    {
        var seededUsers = UserSeed.All.OfType<ApplicationUser>().ToList();
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await api.SearchUsersAsync("", 1, 50);

        result.TotalCount.Should().Be(seededUsers.Count);
        result.Items.Select(user => user.Id).Should().Contain(seededUsers.Select(user => user.Id));
    }

    [Fact]
    public async Task Search_narrows_to_users_whose_email_contains_the_text_regardless_of_letter_case()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await api.SearchUsersAsync("MARKO", 1, 50);

        result.Items.Should().ContainSingle(user => user.Id == UserSeed.MarkoNovak.Id);
        result.Items.Should().NotContain(user => user.Id == UserSeed.AnaJovic.Id);
        result.Items.Should().NotContain(user => user.Id == UserSeed.Explorer.Id);
    }

    [Fact]
    public async Task Second_page_of_a_search_returns_users_after_the_first_page_and_the_total_stays_the_same()
    {
        var seededUsers = UserSeed.All.OfType<ApplicationUser>().OrderBy(user => user.Email, StringComparer.Ordinal).ToList();
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await api.SearchUsersAsync("@test.com", 2, 1);

        result.TotalCount.Should().Be(seededUsers.Count);
        result.Items.Select(user => user.Id).Should().Equal(seededUsers[1].Id);
    }

    [Fact]
    public async Task Users_by_ids_returns_the_found_users_and_omits_the_ones_that_do_not_exist()
    {
        var missingId = Guid.NewGuid();
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await api.GetUsersAsync([UserSeed.Explorer.Id, UserSeed.MarkoNovak.Id, missingId]);

        result.Select(user => user.Id).Should().BeEquivalentTo([UserSeed.Explorer.Id, UserSeed.MarkoNovak.Id]);
        result.Should().Contain(user => user.Id == UserSeed.MarkoNovak.Id && user.DisplayName == UserSeed.MarkoNovak.Email);
    }
}
