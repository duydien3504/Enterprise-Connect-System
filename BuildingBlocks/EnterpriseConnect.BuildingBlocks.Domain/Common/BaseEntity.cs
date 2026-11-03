namespace EnterpriseConnect.BuildingBlocks.Domain.Common;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
}

public interface IEntity : IEntity<Guid>
{
}

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    Guid? CreatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}

public abstract class BaseEntity<TKey> : IEntity<TKey>, IAuditableEntity, ISoftDeletable
{
    public virtual TKey Id { get; set; } = default!;

    public virtual DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public virtual Guid? CreatedBy { get; set; }

    public virtual DateTimeOffset? UpdatedAt { get; set; }
    public virtual Guid? UpdatedBy { get; set; }

    public virtual bool IsDeleted { get; set; } = false;
    public virtual DateTimeOffset? DeletedAt { get; set; }
    public virtual Guid? DeletedBy { get; set; }
}

public abstract class BaseEntity : BaseEntity<Guid>, IEntity
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}
