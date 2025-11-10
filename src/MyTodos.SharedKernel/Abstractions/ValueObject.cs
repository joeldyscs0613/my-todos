namespace MyTodos.SharedKernel.Abstractions;

/// <summary>
/// Base class for value objects in the domain model.
/// Value objects are immutable and compared by their component values rather than identity.
/// </summary>
/// <remarks>
/// Derived classes must implement <see cref="GetEqualityComponents"/> to define which properties
/// participate in equality comparisons. All properties should be immutable (init-only or private set).
/// </remarks>
public abstract class ValueObject
{
    /// <summary>
    /// Returns the components that define equality for this value object.
    /// </summary>
    /// <returns>An enumerable of components used for equality comparison and hash code calculation.</returns>
    /// <remarks>
    /// Override this method to yield the properties that should be compared for equality.
    /// The order of components matters - components are compared sequentially.
    /// </remarks>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// </summary>
    /// <param name="obj">The object to compare with the current value object.</param>
    /// <returns>true if the specified object is equal to the current value object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Returns the hash code for this value object.
    /// </summary>
    /// <returns>A hash code computed from all equality components.</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate(1, (current, hash) => unchecked(current * 23 + hash));
    }

    /// <summary>
    /// Determines whether two value objects are equal.
    /// </summary>
    /// <param name="left">The first value object to compare.</param>
    /// <param name="right">The second value object to compare.</param>
    /// <returns>true if the value objects are equal; otherwise, false.</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two value objects are not equal.
    /// </summary>
    /// <param name="left">The first value object to compare.</param>
    /// <param name="right">The second value object to compare.</param>
    /// <returns>true if the value objects are not equal; otherwise, false.</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
