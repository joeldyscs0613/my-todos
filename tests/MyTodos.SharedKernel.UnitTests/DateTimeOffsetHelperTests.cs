using MyTodos.SharedKernel.Helpers;

namespace MyTodos.SharedKernel.UnitTests;

public class DateTimeOffsetHelperTests
{
    [Fact]
    public void UtcNow_ReturnsUtcTime()
    {
        // Act
        var result = DateTimeOffsetHelper.UtcNow;

        // Assert
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    [Fact]
    public void UtcNow_ReturnsCurrentTime()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var result = DateTimeOffsetHelper.UtcNow;

        // Arrange
        var after = DateTimeOffset.UtcNow;

        // Assert - Result should be within 1-second window
        Assert.True(result >= before);
        Assert.True(result <= after);
        Assert.True((after - before).TotalSeconds < 1);
    }
}
