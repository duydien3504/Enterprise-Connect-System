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
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>(entity =>
        {
            entity.Property(d => d.Title).HasMaxLength(300).IsRequired();
            entity.Property(d => d.FileUrl).HasMaxLength(1000).IsRequired();
            entity.Property(d => d.Version).HasMaxLength(20).IsRequired();
            entity.Property(d => d.Description).HasMaxLength(1000);
            entity.HasIndex(d => d.UploaderId);
        });

        modelBuilder.Entity<DocumentAccess>(entity =>
        {
            entity.HasIndex(da => da.RoleId);
            entity.HasIndex(da => da.DepartmentId);
            entity.HasOne(da => da.Document)
                .WithMany(d => d.AccessList)
                .HasForeignKey(da => da.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ToxicViolation>(entity =>
        {
            entity.HasIndex(tv => tv.MessageId);
            entity.HasIndex(tv => tv.SenderId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(al => al.CorrelationId).HasMaxLength(100).IsRequired();
            entity.Property(al => al.ServiceName).HasMaxLength(100).IsRequired();
            entity.Property(al => al.ActionName).HasMaxLength(150).IsRequired();
            entity.Property(al => al.TargetEntity).HasMaxLength(150).IsRequired();
            entity.Property(al => al.IpAddress).HasMaxLength(50).IsRequired();
            entity.Property(al => al.HttpMethod).HasMaxLength(10).IsRequired();
            entity.Property(al => al.EndpointUrl).HasMaxLength(500).IsRequired();
            entity.HasIndex(al => al.CorrelationId);
            entity.HasIndex(al => al.Timestamp);
            entity.HasIndex(al => al.ActorId);
            entity.HasIndex(al => al.RiskLevel);
            entity.HasIndex(al => al.ServiceName);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.Property(s => s.SettingKey).HasMaxLength(100).IsRequired();
            entity.Property(s => s.GroupName).HasMaxLength(100).IsRequired();
            entity.HasIndex(s => s.SettingKey).IsUnique();
        });
    }
}
