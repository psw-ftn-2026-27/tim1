using Games.Application;
using Games.Application.ChallengeImages;
using Games.Application.ChallengeOverview;
using Games.Application.Images;
using Games.Application.LocationChallengeManagement;
using Games.Application.LocationChallenges;
using Games.Application.ChallengeCompletions;
using Games.Application.QuizAttempts;
using Games.Application.QuizManagement;
using Games.Application.QuizPlaying;
using Games.Application.Quizzes;
using Games.Application.SocialChallengeManagement;
using Games.Application.SocialChallenges;
using Games.Domain.QuizAttempts;
using Games.Infrastructure.Images;
using Games.Infrastructure.Persistence;
using Games.Infrastructure.Persistence.ChallengeCompletions;
using Games.Infrastructure.Persistence.ChallengeOverview;
using Games.Infrastructure.Persistence.Images;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.QuizAttempts;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Games.Infrastructure;

public static class GamesModuleExtensions
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<GamesDbContext>(configuration, "games");
        services.AddHostedService<GamesModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<GamesMapperProfile>());

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<GamesDbContext>());
        services.AddScoped<ISocialChallengeRepository, SocialChallengeRepository>();
        services.AddScoped<ISocialChallengeReadRepository, SocialChallengeReadRepository>();

        services.AddScoped<SocialChallengeManagementService>();
        services.AddScoped<SocialChallengeManagementQueries>();

        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizReadRepository, QuizReadRepository>();
        services.AddScoped<QuizManagementService>();
        services.AddScoped<QuizManagementQueries>();
        services.AddScoped<IChallengeOverviewReadRepository, ChallengeOverviewReadRepository>();
        services.AddScoped<ChallengeOverviewQueries>();

        services.AddScoped<ILocationChallengeRepository, LocationChallengeRepository>();
        services.AddScoped<ILocationChallengeReadRepository, LocationChallengeReadRepository>();
        services.AddScoped<LocationChallengeManagementService>();
        services.AddScoped<LocationChallengeManagementQueries>();

        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IImageReadRepository, ImageReadRepository>();
        services.AddSingleton<IImageProcessor, ImageProcessorAdapter>();
        services.AddSingleton<IImageStorage, ImageStorageAdapter>();
        services.AddScoped<ChallengeImagesService>();
        services.AddScoped<ChallengeImagesQueries>();
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<IChallengeCompletionRepository, ChallengeCompletionRepository>();
        services.AddScoped<QuizActivation>();
        services.AddScoped<QuizAnswering>();
        services.AddScoped<QuizPlayingService>();
        services.AddScoped<QuizPlayingQueries>();

        return services;
    }
}
