using EnterpriseConnect.BuildingBlocks.Infrastructure.Persistence;
using EnterpriseConnect.MediaService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.MediaService.Infrastructure.Persistence.Context;

public class MediaDbContext : BaseDbContext
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options)
    {
    }

    public DbSet<CallSession> CallSessions => Set<CallSession>();
    public DbSet<CallParticipant> CallParticipants => Set<CallParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure CallSession
        modelBuilder.Entity<CallSession>(entity =>
        {
            entity.HasIndex(cs => cs.ConversationId);
            entity.HasIndex(cs => cs.InitiatorId);
        });

        // Configure CallParticipant
        modelBuilder.Entity<CallParticipant>(entity =>
        {
            entity.HasIndex(cp => new { cp.CallId, cp.UserId });

            entity.HasOne(cp => cp.CallSession)
                .WithMany(cs => cs.Participants)
                .HasForeignKey(cp => cp.CallId);
        });
    }
}
