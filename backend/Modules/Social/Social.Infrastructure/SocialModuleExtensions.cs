using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Social.Application;
using Social.Application.BlogAuthoring;
using Social.Application.BlogCommenting;
using Social.Application.BlogReading;
using Social.Application.ClubBrowsing;
using Social.Application.Blogs;
using Social.Application.ClubManagement;
using Social.Application.Clubs;
using Social.Application.FollowRequesting;
using Social.Application.FollowRequests;
using Social.Application.Images;
using Social.Infrastructure.Images;
using Social.Infrastructure.Persistence;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.Clubs;
using Social.Infrastructure.Persistence.FollowRequests;
using Social.Infrastructure.Persistence.Images;

namespace Social.Infrastructure;

public static class SocialModuleExtensions
{
    public static IServiceCollection AddSocialModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<SocialDbContext>(configuration, "social");
        services.AddHostedService<SocialModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<SocialMapperProfile>());

        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SocialDbContext>());
        services.AddScoped<IBlogReadRepository, BlogReadRepository>();
        services.AddScoped<IClubRepository, ClubRepository>();
        services.AddScoped<IClubReadRepository, ClubReadRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IImageReadRepository, ImageReadRepository>();
        services.AddSingleton<IImageProcessor, ImageProcessorAdapter>();
        services.AddSingleton<IImageStorage, ImageStorageAdapter>();

        services.AddScoped<IFollowRequestRepository, FollowRequestRepository>();
        services.AddScoped<IFollowRequestReadRepository, FollowRequestReadRepository>();

        services.AddScoped<BlogAuthoringService>();
        services.AddScoped<BlogAuthoringQueries>();
        services.AddScoped<BlogReadingQueries>();
        services.AddScoped<BlogCommentingService>();
        services.AddScoped<ClubManagementService>();
        services.AddScoped<ClubManagementQueries>();
        services.AddScoped<ClubBrowsingQueries>();
        services.AddScoped<FollowRequestingService>();
        services.AddScoped<FollowRequestingQueries>();

        return services;
    }
}
