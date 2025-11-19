using MyTodos.Services.IdentityService.Domain.UserAggregate.DomainEvents;
using MyTodos.SharedKernel;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Domain.UserAggregate;

/// <summary>
/// Represents a user in the system.
/// Users can have global roles or be assigned to specific tenants via UserRole.
/// </summary>
public sealed class User : AggregateRoot<Guid>
{
    /// <summary>
    /// Unique username
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Hashed password
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string? FirstName { get; private set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public string? LastName { get; private set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Last time the user logged in
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; private set; }

    // Child entities within User aggregate
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

    // Private parameterless constructor for EF Core
    [Obsolete("Only for deserialization. Use Create method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private User() : base() { }

    // Constructor for factory method
    private User(Guid id) : base(id) { }

    /// <summary>
    /// Create a new user
    /// </summary>
    public static Result<User> Create(
        string username,
        string email,
        string passwordHash,
        string? firstName = null,
        string? lastName = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<User>(Error.BadRequest("Username cannot be empty"));

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>(Error.BadRequest("Email cannot be empty"));

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure<User>(Error.BadRequest("PasswordHash cannot be empty"));

        var userId = Guid.NewGuid();
        var user = new User(userId)
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true
        };

        user.AddDomainEvent(new UserRegisteredDomainEvent(user.Id, user.Username, user.Email));

        return Result.Success(user);
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    public void UpdateProfile(string? firstName, string? lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Update user password
    /// </summary>
    public Result UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            return Result.Failure(Error.BadRequest("Password cannot be empty"));

        PasswordHash = newPasswordHash;
        return Result.Success();
    }

    /// <summary>
    /// Record a login event
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTimeOffsetHelper.UtcNow;
        AddDomainEvent(new UserLoggedInDomainEvent(Id, DateTimeOffsetHelper.UtcNow));
    }

    /// <summary>
    /// Deactivate the user account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activate the user account
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Assign a global role to this user (no tenant restriction)
    /// </summary>
    /// <param name="roleId">The ID of the role to assign</param>
    public void AssignGlobalRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId && ur.TenantId == null))
            return; // Already has this global role

        var userRole = UserRole.CreateGlobalRole(Id, roleId);
        _userRoles.Add(userRole);

        AddDomainEvent(new GlobalRoleAssignedDomainEvent(Id, roleId));
    }

    /// <summary>
    /// Assign a tenant-specific role to this user
    /// </summary>
    /// <param name="roleId">The ID of the role to assign</param>
    /// <param name="tenantId">The ID of the tenant this role applies to</param>
    public void AssignTenantRole(Guid roleId, Guid tenantId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId && ur.TenantId == tenantId))
            return; // Already has this role in this tenant

        var userRole = UserRole.CreateTenantRole(Id, roleId, tenantId);
        _userRoles.Add(userRole);

        AddDomainEvent(new TenantRoleAssignedDomainEvent(Id, roleId, tenantId));
    }

    /// <summary>
    /// Remove a role from this user
    /// </summary>
    /// <param name="roleId">The ID of the role to remove</param>
    /// <param name="tenantId">The tenant ID (null for global roles)</param>
    public void RemoveRole(Guid roleId, Guid? tenantId = null)
    {
        var userRole = _userRoles.FirstOrDefault(ur =>
            ur.RoleId == roleId && ur.TenantId == tenantId);

        if (userRole != null)
        {
            _userRoles.Remove(userRole);
        }
    }

    /// <summary>
    /// Get all tenant IDs this user belongs to
    /// </summary>
    public IEnumerable<Guid> GetTenantIds()
    {
        return _userRoles
            .Where(ur => ur.TenantId.HasValue)
            .Select(ur => ur.TenantId!.Value)
            .Distinct();
    }

    /// <summary>
    /// Check if user has a global role
    /// </summary>
    public bool HasGlobalRole(Guid roleId)
    {
        return _userRoles.Any(ur => ur.RoleId == roleId && ur.TenantId == null);
    }

    /// <summary>
    /// Check if user has a specific role in a specific tenant
    /// </summary>
    public bool HasTenantRole(Guid roleId, Guid tenantId)
    {
        return _userRoles.Any(ur => ur.RoleId == roleId && ur.TenantId == tenantId);
    }
}
