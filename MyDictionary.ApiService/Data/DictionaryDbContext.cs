using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Models;

namespace MyDictionary.ApiService.Data;

public class DictionaryDbContext : DbContext
{
    public DictionaryDbContext(DbContextOptions<DictionaryDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Entry> Entries { get; set; }
    public DbSet<EntryFavorite> EntryFavorites { get; set; }
    public DbSet<EntryLink> EntryLinks { get; set; }
    public DbSet<UserAgreement> UserAgreements { get; set; }
    public DbSet<UserAgreementAcceptance> UserAgreementAcceptances { get; set; }
    public DbSet<FriendRequest> FriendRequests { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<UserBlock> UserBlocks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


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

        // Configure FriendRequest relationships
        modelBuilder.Entity<FriendRequest>()
            .HasOne(fr => fr.Sender)
            .WithMany()
            .HasForeignKey(fr => fr.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FriendRequest>()
            .HasOne(fr => fr.Receiver)
            .WithMany()
            .HasForeignKey(fr => fr.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // Aynı kişiye birden fazla istek gönderilmesini engelle
        modelBuilder.Entity<FriendRequest>()
            .HasIndex(fr => new { fr.SenderId, fr.ReceiverId, fr.Status })
            .IsUnique();

        // Configure Friendship relationships
        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Friend)
            .WithMany()
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.Restrict);

        // Aynı arkadaşlık ilişkisini birden fazla kez oluşturmayı engelle
        modelBuilder.Entity<Friendship>()
            .HasIndex(f => new { f.UserId, f.FriendId })
            .IsUnique();

        // Configure Notification relationships
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.FromUser)
            .WithMany()
            .HasForeignKey(n => n.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Add index for better query performance
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

        // Configure Message relationships
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // Add indexes for better query performance
        modelBuilder.Entity<Message>()
            .HasIndex(m => new { m.SenderId, m.ReceiverId, m.CreatedAt });

        modelBuilder.Entity<Message>()
            .HasIndex(m => new { m.ReceiverId, m.IsRead });

        // Configure Conversation relationships
        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.User1)
            .WithMany()
            .HasForeignKey(c => c.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.User2)
            .WithMany()
            .HasForeignKey(c => c.User2Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.LastMessage)
            .WithMany()
            .HasForeignKey(c => c.LastMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ensure unique conversation between two users
        modelBuilder.Entity<Conversation>()
            .HasIndex(c => new { c.User1Id, c.User2Id })
            .IsUnique();

        // Add index for conversation queries
        modelBuilder.Entity<Conversation>()
            .HasIndex(c => new { c.User1Id, c.LastMessageAt });

        modelBuilder.Entity<Conversation>()
            .HasIndex(c => new { c.User2Id, c.LastMessageAt });

        // Configure Category relationships
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        // Configure Topic relationships - Updated
        modelBuilder.Entity<Topic>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Topics)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Topic>()
            .HasOne(t => t.CreatedByUser)
            .WithMany(u => u.Topics)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Topic>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        // Configure Entry relationships - Updated
        modelBuilder.Entity<Entry>()
            .HasOne(e => e.Topic)
            .WithMany(t => t.Entries)
            .HasForeignKey(e => e.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Entry>()
            .HasOne(e => e.CreatedByUser)
            .WithMany(u => u.Entries)
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure EntryFavorite relationships
        modelBuilder.Entity<EntryFavorite>()
            .HasOne(ef => ef.User)
            .WithMany(u => u.FavoriteEntries)
            .HasForeignKey(ef => ef.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EntryFavorite>()
            .HasOne(ef => ef.Entry)
            .WithMany(e => e.Favorites)
            .HasForeignKey(ef => ef.EntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Prevent duplicate favorites
        modelBuilder.Entity<EntryFavorite>()
            .HasIndex(ef => new { ef.UserId, ef.EntryId })
            .IsUnique();

        // Configure EntryLink relationships
        modelBuilder.Entity<EntryLink>()
            .HasOne(el => el.Entry)
            .WithMany(e => e.Links)
            .HasForeignKey(el => el.EntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance indexes
        modelBuilder.Entity<Topic>()
            .HasIndex(t => new { t.CategoryId, t.LastEntryAt });

        modelBuilder.Entity<Entry>()
            .HasIndex(e => new { e.TopicId, e.CreatedAt });

        modelBuilder.Entity<Entry>()
            .HasIndex(e => new { e.CreatedByUserId, e.CreatedAt });

        modelBuilder.Entity<EntryFavorite>()
            .HasIndex(ef => new { ef.UserId, ef.CreatedAt });

        // Configure UserBlock relationships
        modelBuilder.Entity<UserBlock>()
            .HasOne(ub => ub.BlockingUser)
            .WithMany()
            .HasForeignKey(ub => ub.BlockingUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserBlock>()
            .HasOne(ub => ub.BlockedUser)
            .WithMany()
            .HasForeignKey(ub => ub.BlockedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate blocks
        modelBuilder.Entity<UserBlock>()
            .HasIndex(ub => new { ub.BlockingUserId, ub.BlockedUserId })
            .IsUnique();
    }
}