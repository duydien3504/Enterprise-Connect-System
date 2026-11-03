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
    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();
    public DbSet<AiSummary> AiSummaries => Set<AiSummary>();
    public DbSet<KnowledgeChunk> KnowledgeChunks => Set<KnowledgeChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Conversation
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(250);
            entity.Property(c => c.AvatarUrl).HasMaxLength(500);
        });

        // Configure ConversationMember
        modelBuilder.Entity<ConversationMember>(entity =>
        {
            entity.HasIndex(cm => new { cm.ConversationId, cm.UserId }).IsUnique();
            entity.Property(cm => cm.Nickname).HasMaxLength(100);

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

        // Configure MessageReaction
        modelBuilder.Entity<MessageReaction>(entity =>
        {
            entity.Property(r => r.Emoji).HasMaxLength(32).IsRequired();
            entity.HasIndex(r => new { r.MessageId, r.UserId, r.Emoji }).IsUnique();

            entity.HasOne(r => r.Message)
                .WithMany(m => m.Reactions)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MessageAttachment
        modelBuilder.Entity<MessageAttachment>(entity =>
        {
            entity.Property(a => a.FileUrl).HasMaxLength(500).IsRequired();
            entity.Property(a => a.FileName).HasMaxLength(255).IsRequired();
            entity.Property(a => a.ContentType).HasMaxLength(100);

            entity.HasOne(a => a.Message)
                .WithMany(m => m.Attachments)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure AiSummary
        modelBuilder.Entity<AiSummary>(entity =>
        {
            entity.HasOne(s => s.Conversation)
                .WithMany(c => c.Summaries)
                .HasForeignKey(s => s.ConversationId);
        });

        // Configure KnowledgeChunk
        modelBuilder.Entity<KnowledgeChunk>(entity =>
        {
            entity.Property(k => k.DocumentTitle).HasMaxLength(250).IsRequired();
            entity.Property(k => k.Content).IsRequired();
        });
    }
}
