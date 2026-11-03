using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.AdminService.Infrastructure.Persistence.Context;

public class AdminDbContext : BaseDbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentAccess> DocumentAccesses => Set<DocumentAccess>();
    public DbSet<ToxicViolation> ToxicViolations => Set<ToxicViolation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Document
        modelBuilder.Entity<Document>(entity =>
        {
            entity.Property(d => d.Title).HasMaxLength(300).IsRequired();
            entity.Property(d => d.FileUrl).HasMaxLength(1000).IsRequired();
            entity.HasIndex(d => d.UploaderId);
        });

        // Configure DocumentAccess
        modelBuilder.Entity<DocumentAccess>(entity =>
        {
            entity.HasIndex(da => da.RoleId);
            entity.HasIndex(da => da.DepartmentId);

            entity.HasOne(da => da.Document)
                .WithMany(d => d.AccessList)
                .HasForeignKey(da => da.DocumentId);
        });

        // Configure ToxicViolation
        modelBuilder.Entity<ToxicViolation>(entity =>
        {
            entity.HasIndex(tv => tv.MessageId);
            entity.HasIndex(tv => tv.SenderId);
        });

        // Configure AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(al => al.ActionName).HasMaxLength(150).IsRequired();
            entity.Property(al => al.TargetEntity).HasMaxLength(150).IsRequired();
            entity.HasIndex(al => al.ActorId);
        });
    }
}
