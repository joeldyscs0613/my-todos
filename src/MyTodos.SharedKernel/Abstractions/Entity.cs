using MyTodos.SharedKernel.Helpers;

namespace MyTodos.SharedKernel.Abstractions;

/// <summary>
/// Base class for all entities in the domain model.
/// Entities have a unique identity and audit trail properties.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId>
    where TId : IComparable
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public TId Id { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when this entity was created.
    /// </summary>
    public DateTimeOffset CreatedDate { get; private set; }

    /// <summary>
    /// Gets the username of the user who created this entity.
    /// </summary>
    public string CreatedBy { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when this entity was last modified, or null if never modified.
    /// </summary>
    public DateTimeOffset? ModifiedDate { get; private set; }

    /// <summary>
    /// Gets the username of the user who last modified this entity, or null if never modified.
    /// </summary>
    public string? ModifiedBy { get; private set; }

    /// <summary>
    /// Initializes a new instance of the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    protected Entity(TId id)
    {
        Id = id;
    }
    
    /// <summary>
    /// Parameterless constructor for deserialization only (EF Core, JSON serializers).
    /// DO NOT USE in domain code.
    /// </summary>
    [Obsolete("Only for deserialization. Use parameterized constructor.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    protected Entity()
    {
    }

    /// <summary>
    /// Sets the creation audit information for this entity.
    /// </summary>
    /// <param name="username">The username of the user creating this entity.</param>
    /// <exception cref="ArgumentException">Thrown when username is null, empty, or whitespace.</exception>
    public void SetCreatedInfo(string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));

        CreatedBy = username;
        CreatedDate = DateTimeOffsetHelper.UtcNow;
    }

    /// <summary>
    /// Sets the modification audit information for this entity.
    /// </summary>
    /// <param name="username">The username of the user modifying this entity.</param>
    /// <exception cref="ArgumentException">Thrown when username is null, empty, or whitespace.</exception>
    public void SetUpdatedInfo(string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));

        ModifiedBy = username;
        ModifiedDate = DateTimeOffsetHelper.UtcNow;
    }
}