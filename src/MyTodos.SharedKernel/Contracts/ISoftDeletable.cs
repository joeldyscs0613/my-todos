namespace MyTodos.SharedKernel.Contracts;

/// <summary>
/// Marker interface for entities that support soft delete functionality.
/// Entities implementing this interface will have soft delete query filters applied automatically.
/// In practice, real world project, I would create a SoftDelete entity/agggreage root and implement methods.
/// </summary>
/// <remarks>
/// Soft delete marks entities as deleted without physically removing them from the database,
/// allowing for data recovery and maintaining audit trails.
/// Null or false values indicate the entity is active; true indicates it is deleted.
/// </remarks>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates whether this entity has been soft deleted.
    /// Null or false = active, true = deleted.
    /// </summary>
    bool? IsDeleted { get; }

    /// <summary>
    /// Soft deletes the item.
    /// </summary>
    void Delete();

    /// <summary>
    /// Restores a soft deleted item.
    /// </summary>
    void Restore();
}
