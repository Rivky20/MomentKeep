using Microsoft.EntityFrameworkCore;
using MomentKeep.Core.Models;

namespace MomentKeep.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Folder> Trips { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ImageTag> ImageTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
                entity.Property(e => e.StorageQuota).HasDefaultValue(10240);
                entity.Property(e => e.AiQuota).HasDefaultValue(50);
            });

            // Trip configuration
            modelBuilder.Entity<Folder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasColumnType("text");
                entity.Property(e => e.ShareId).IsRequired(false);
                entity.HasIndex(e => e.ShareId).IsUnique();
                entity.Property(e => e.Latitude).HasColumnType("decimal(10,8)").IsRequired(false);
                entity.Property(e => e.Longitude).HasColumnType("decimal(11,8)").IsRequired(false);
                entity.Property(e => e.LocationName).HasMaxLength(100).IsRequired(false);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Trips)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Image configuration
            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(255).IsRequired();
                entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.AiPrompt).HasColumnType("text").IsRequired(false);
                entity.Property(e => e.AiStyle).HasMaxLength(50).IsRequired(false);

                entity.HasOne(e => e.Trip)
                    .WithMany(t => t.Images)
                    .HasForeignKey(e => e.TripId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Images)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Tag configuration
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // ImageTag configuration (junction table)
            modelBuilder.Entity<ImageTag>(entity =>
            {
                entity.HasKey(e => new { e.ImageId, e.TagId });

                entity.HasOne(e => e.Image)
                    .WithMany(i => i.ImageTags)
                    .HasForeignKey(e => e.ImageId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.ImageTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}