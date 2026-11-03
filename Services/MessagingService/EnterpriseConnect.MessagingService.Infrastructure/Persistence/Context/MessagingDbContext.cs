using EnterpriseConnect.BuildingBlocks.Infrastructure.Persistence;
using EnterpriseConnect.MessagingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.MessagingService.Infrastructure.Persistence.Context;

public class MessagingDbContext : BaseDbContext
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options) : base(options)
    {
    }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<AiSummary> AiSummaries => Set<AiSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Conversation
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(200);
            entity.Property(c => c.AvatarUrl).HasMaxLength(500);
        });

        // Configure ConversationMember
        modelBuilder.Entity<ConversationMember>(entity =>
        {
            entity.HasIndex(cm => new { cm.ConversationId, cm.UserId }).IsUnique();

            entity.HasOne(cm => cm.Conversation)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.ConversationId);
        });

        // Configure Message
        modelBuilder.Entity<Message>(entity =>
        {
            entity.Property(m => m.Content).IsRequired();

            entity.HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId);

            entity.HasOne(m => m.ReplyTo)
                .WithMany(m => m.Replies)
                .HasForeignKey(m => m.ReplyToId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure AiSummary
        modelBuilder.Entity<AiSummary>(entity =>
        {
            entity.HasOne(s => s.Conversation)
                .WithMany(c => c.Summaries)
                .HasForeignKey(s => s.ConversationId);
        });
    }
}
