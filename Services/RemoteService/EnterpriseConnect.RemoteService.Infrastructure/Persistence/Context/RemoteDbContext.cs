using EnterpriseConnect.BuildingBlocks.Infrastructure.Persistence;
using EnterpriseConnect.RemoteService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.RemoteService.Infrastructure.Persistence.Context;

public class RemoteDbContext : BaseDbContext
{
    public RemoteDbContext(DbContextOptions<RemoteDbContext> options) : base(options)
    {
    }

    public DbSet<RemoteSession> RemoteSessions => Set<RemoteSession>();
    public DbSet<AccessGrant> AccessGrants => Set<AccessGrant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure RemoteSession
        modelBuilder.Entity<RemoteSession>(entity =>
        {
            entity.Property(rs => rs.SessionCode).HasMaxLength(20).IsRequired();
            entity.HasIndex(rs => rs.SessionCode).IsUnique();
            entity.HasIndex(rs => rs.RequesterId);
            entity.HasIndex(rs => rs.SupporterId);
        });

        // Configure AccessGrant
        modelBuilder.Entity<AccessGrant>(entity =>
        {
            entity.HasOne(ag => ag.Session)
                .WithMany(s => s.AccessGrants)
                .HasForeignKey(ag => ag.SessionId);
        });
    }
}
