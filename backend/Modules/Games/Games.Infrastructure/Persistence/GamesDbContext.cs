using Games.Application;
using Games.Domain.Images;
using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence.Images;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence;

internal sealed class GamesDbContext : DbContext, IUnitOfWork
{
    public GamesDbContext(DbContextOptions<GamesDbContext> options) : base(options) { }

    public DbSet<SocialChallenge> SocialChallenges => Set<SocialChallenge>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<LocationChallenge> LocationChallenges => Set<LocationChallenge>();
    public DbSet<Image> Images => Set<Image>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("games");
        builder.ConfigureSocialChallenges();
        builder.ConfigureQuizzes();
        builder.ConfigureLocationChallenges();
        builder.ConfigureImages();
    }
}
