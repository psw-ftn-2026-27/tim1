using Microsoft.EntityFrameworkCore;
using Social.Application;
using Social.Domain.Blogs;
using Social.Domain.FollowRequests;
using Social.Domain.Images;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.FollowRequests;
using Social.Infrastructure.Persistence.Images;

namespace Social.Infrastructure.Persistence;

internal sealed class SocialDbContext : DbContext, IUnitOfWork
{
    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<FollowRequest> FollowRequests => Set<FollowRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("social");
        builder.ConfigureBlogs();
        builder.ConfigureImages();
        builder.ConfigureFollowRequests();
    }
}
