using Exploration.Application;
using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence.TourPreferences;
using Exploration.Infrastructure.Persistence.Tours;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationDbContext : DbContext, IUnitOfWork
{
    public ExplorationDbContext(DbContextOptions<ExplorationDbContext> options) : base(options) { }

    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<TourPreference> TourPreferences => Set<TourPreference>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("exploration");
        builder.ConfigureTours();
        builder.ConfigureTourPreferences();
    }
}
