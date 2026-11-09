using Microsoft.EntityFrameworkCore;
using Social.Application;
using Social.Domain.Blogs;
using Social.Domain.Images;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.Images;

namespace Social.Infrastructure.Persistence;

internal sealed class SocialDbContext : DbContext, IUnitOfWork
{
    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Image> Images => Set<Image>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("social");
        builder.ConfigureBlogs();
        builder.ConfigureImages();
    }
}
