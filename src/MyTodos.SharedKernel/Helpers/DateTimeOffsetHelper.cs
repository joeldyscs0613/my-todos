namespace MyTodos.SharedKernel.Helpers;

/// <summary>
/// Helper class for working with DateTimeOffset values.
/// Provides testable access to current UTC time.
/// </summary>
public static class DateTimeOffsetHelper
{
    /// <summary>
    /// Gets the current UTC time as a DateTimeOffset.
    /// </summary>
    /// <remarks>
    /// This property wraps DateTimeOffset.UtcNow to enable easier testing by allowing
    /// test code to potentially replace this helper with a test double.
    /// </remarks>
    public static DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}