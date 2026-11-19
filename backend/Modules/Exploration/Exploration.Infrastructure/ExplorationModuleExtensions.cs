using Exploration.Application;
using Exploration.Application.Images;
using Exploration.Application.KeypointAuthoring;
using Exploration.Application.KeypointImageUpload;
using Exploration.Application.TourAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.TourPreferenceEditing;
using Exploration.Application.TourPreferences;
using Exploration.Application.Tours;
using Exploration.Infrastructure.Images;
using Exploration.Application.UserPositioning;
using Exploration.Application.UserPositions;
using Exploration.Contracts;
using Exploration.Infrastructure.Persistence;
using Exploration.Infrastructure.Persistence.Images;
using Exploration.Infrastructure.Persistence.TourPreferences;
using Exploration.Infrastructure.Persistence.Tours;
using Exploration.Infrastructure.Persistence.UserPositions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Exploration.Infrastructure;

public static class ExplorationModuleExtensions
{
    public static IServiceCollection AddExplorationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<ExplorationDbContext>(configuration, "exploration");
        services.AddHostedService<ExplorationModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<ExplorationMapperProfile>());

        services.AddScoped<ITourRepository, TourRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ExplorationDbContext>());
        services.AddScoped<ITourReadRepository, TourReadRepository>();

        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IImageReadRepository, ImageReadRepository>();
        services.AddSingleton<IImageProcessor, ImageProcessorAdapter>();
        services.AddSingleton<IImageStorage, ImageStorageAdapter>();

        services.AddScoped<ITourPreferenceRepository, TourPreferenceRepository>();
        services.AddScoped<ITourPreferenceReadRepository, TourPreferenceReadRepository>();
        services.AddScoped<IUserPositionRepository, UserPositionRepository>();
        services.AddScoped<IUserPositionReadRepository, UserPositionReadRepository>();

        services.AddScoped<TourAuthoringService>();
        services.AddScoped<TourAuthoringQueries>();
        services.AddScoped<KeypointAuthoringService>();
        services.AddScoped<KeypointAuthoringQueries>();
        services.AddScoped<KeypointImageUploadService>();
        services.AddScoped<TourBrowsingQueries>();
        services.AddScoped<TourPreferenceEditingService>();
        services.AddScoped<TourPreferenceEditingQueries>();
        services.AddScoped<UserPositioningService>();
        services.AddScoped<UserPositioningQueries>();
        services.AddScoped<IExplorationApi, ExplorationApi>();

        return services;
    }
}
