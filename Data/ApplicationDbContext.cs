using BlogApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Data
{
    // IdentityDbContext<ApplicationUser> уже даёт нам DbSet<ApplicationUser> Users,
    // DbSet<IdentityRole> Roles, а также таблицы UserRoles, UserClaims и т.д.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<ArticleTag> ArticleTags { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---- ArticleTag: составной ключ и связь многие-ко-многим ----
            builder.Entity<ArticleTag>(entity =>
            {
                entity.HasKey(at => new { at.ArticleId, at.TagId });

                entity.HasOne(at => at.Article)
                    .WithMany(a => a.ArticleTags)
                    .HasForeignKey(at => at.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(at => at.Tag)
                    .WithMany(t => t.ArticleTags)
                    .HasForeignKey(at => at.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Article -> Author (ApplicationUser) ----
            builder.Entity<Article>(entity =>
            {
                entity.HasOne(a => a.Author)
                    .WithMany(u => u.Articles)
                    .HasForeignKey(a => a.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => a.Title);
            });

            // ---- Tag -> CreatedByUser (ApplicationUser) ----
            builder.Entity<Tag>(entity =>
            {
                entity.HasOne(t => t.CreatedByUser)
                    .WithMany(u => u.CreatedTags)
                    .HasForeignKey(t => t.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => t.Name).IsUnique();
            });

            // ---- Comment -> Article / User ----
            builder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.Article)
                    .WithMany(a => a.Comments)
                    .HasForeignKey(c => c.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
