using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Models;

namespace MyDictionary.ApiService.Data;

public class DictionaryDbContext : DbContext
{
    public DictionaryDbContext(DbContextOptions<DictionaryDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Entry> Entries { get; set; }
    public DbSet<UserAgreement> UserAgreements { get; set; }
    public DbSet<UserAgreementAcceptance> UserAgreementAcceptances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the relationship between Entry and User
        modelBuilder.Entity<Entry>()
            .HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        // Configure the relationship between Entry and Topic
        modelBuilder.Entity<Entry>()
            .HasOne(e => e.Topic)
            .WithMany()
            .HasForeignKey(e => e.TopicId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete entries when a topic is deleted

        // Configure the relationship between Topic and User
        modelBuilder.Entity<Topic>()
            .HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        // Configure UserAgreementAcceptance relationships
        modelBuilder.Entity<UserAgreementAcceptance>()
            .HasOne(uaa => uaa.User)
            .WithMany()
            .HasForeignKey(uaa => uaa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserAgreementAcceptance>()
            .HasOne(uaa => uaa.Agreement)
            .WithMany()
            .HasForeignKey(uaa => uaa.AgreementId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}