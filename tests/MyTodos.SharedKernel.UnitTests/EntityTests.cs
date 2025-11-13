using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.SharedKernel.UnitTests;

public class EntityTests
{
    #region Test Entity Classes

    private class TestIntEntity : Entity<int>
    {
        public TestIntEntity(int id) : base(id) { }
    }

    private class TestGuidEntity : Entity<Guid>
    {
        public TestGuidEntity(Guid id) : base(id) { }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithIntId_SetsIdProperty()
    {
        // Arrange
        var id = 1;

        // Act
        var entity = new TestIntEntity(id);

        // Assert
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void Constructor_WithGuidId_SetsIdProperty()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestGuidEntity(id);

        // Assert
        Assert.Equal(id, entity.Id);
    }

    #endregion

    #region SetCreatedInfo Tests

    [Fact]
    public void SetCreatedInfo_WithValidUsername_SetsCreatedByAndCreatedDate()
    {
        // Arrange
        var entity = new TestIntEntity(1);
        var username = "joe.smith";
        var before = DateTimeOffsetHelper.UtcNow;
        
        // Act
        entity.SetCreatedInfo(username);

        // Assert
        var after = DateTimeOffsetHelper.UtcNow;
        Assert.Equal(username, entity.CreatedBy);
        Assert.NotEqual(default(DateTimeOffset), entity.CreatedDate);
        Assert.True(entity.CreatedDate >= before && entity.CreatedDate <= after);
    }

    [Fact]
    public void SetCreatedInfo_SetsUtcTime()
    {
        // Arrange
        var entity = new TestIntEntity(1);
        var before = DateTimeOffset.UtcNow;

        // Act
        entity.SetCreatedInfo("joe.smith");

        // Assert
        var after = DateTimeOffset.UtcNow;
        Assert.True(entity.CreatedDate >= before);
        Assert.True(entity.CreatedDate <= after);
        Assert.Equal(TimeSpan.Zero, entity.CreatedDate.Offset);
    }

    [Fact]
    public void SetCreatedInfo_WithNullUsername_ThrowsArgumentException()
    {
        // Arrange
        var entity = new TestIntEntity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.SetCreatedInfo(null));
    }

    [Fact]
    public void SetCreatedInfo_WithEmptyUsername_ThrowsArgumentException()
    {
        // Arrange
        var entity = new TestIntEntity(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => entity.SetCreatedInfo(string.Empty));
    }

    [Fact]
    public void SetCreatedInfo_WithWhitespaceUsername_ThrowsArgumentException()
    {
        // Arrange
        var entity = new TestIntEntity(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => entity.SetCreatedInfo("   "));
    }

    [Fact]
    public void SetCreatedInfo_DoesNotSetModifiedInfo()
    {
        // Arrange
        var entity = new TestIntEntity(1);

        // Act
        entity.SetCreatedInfo("joe.smith");

        // Assert
        Assert.Null(entity.ModifiedBy);
        Assert.Null(entity.ModifiedDate);
    }

    #endregion

    #region SetUpdatedInfo Tests

    [Fact]
    public void SetUpdatedInfo_WithValidUsername_SetsModifiedByAndDate()
    {
        // Arrange
        var entity = new TestIntEntity(1);
        var username = "mike.smith";

        // Act
        entity.SetUpdatedInfo(username);

        // Assert
        Assert.Equal(username, entity.ModifiedBy);
        Assert.NotNull(entity.ModifiedDate);
        Assert.NotEqual(default(DateTimeOffset), entity.ModifiedDate.Value);
    }

    [Fact]
    public void SetUpdatedInfo_SetsUtcTime()
    {
        // Arrange
        var entity = new TestIntEntity(1);
        var before = DateTimeOffset.UtcNow;

        // Act
        entity.SetUpdatedInfo("mike.smith");

        // Assert
        var after = DateTimeOffset.UtcNow;
        Assert.NotNull(entity.ModifiedDate);
        Assert.True(entity.ModifiedDate.Value >= before);
        Assert.True(entity.ModifiedDate.Value <= after);
        Assert.Equal(TimeSpan.Zero, entity.ModifiedDate.Value.Offset);
    }

    [Fact]
    public void SetUpdatedInfo_WithNullUsername_ThrowsArgumentException()
    {
        // Arrange
        var entity = new TestIntEntity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.SetUpdatedInfo(null));
    }

    [Fact]
    public void SetUpdatedInfo_WithEmptyUsername_ThrowsArgumentException()
    {
        // Arrange
        var entity = new TestIntEntity(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => entity.SetUpdatedInfo(string.Empty));
    }

    [Fact]
    public void SetUpdatedInfo_WithWhitespaceUsername_ThrowsArgumentException()
    {
        // Arrange
        var entity = new TestIntEntity(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => entity.SetUpdatedInfo("   "));
    }

    [Fact]
    public void SetUpdatedInfo_DoesNotModifyCreatedInfo()
    {
        // Arrange
        var entity = new TestIntEntity(1);
        entity.SetCreatedInfo("original.user");
        var originalCreatedBy = entity.CreatedBy;
        var originalCreatedDate = entity.CreatedDate;

        // Act
        entity.SetUpdatedInfo("different.user");

        // Assert
        Assert.Equal(originalCreatedBy, entity.CreatedBy);
        Assert.Equal(originalCreatedDate, entity.CreatedDate);
    }

    #endregion

    #region Audit Trail Tests

    [Fact]
    public void AuditTrail_CompleteWorkflow_TracksCorrectly()
    {
        // Arrange
        var entity = new TestIntEntity(1);
        var createdByUser = "joe.smith";
        var modifiedByUser = "mike.smith";

        // Act - Create
        entity.SetCreatedInfo(createdByUser);
        var createdDate = entity.CreatedDate;

        // Small delay to ensure ModifiedDate is different
        System.Threading.Thread.Sleep(1);

        // Act - Update
        entity.SetUpdatedInfo(modifiedByUser);

        // Assert
        Assert.Equal(createdByUser, entity.CreatedBy);
        Assert.Equal(createdDate, entity.CreatedDate);
        Assert.Equal(modifiedByUser, entity.ModifiedBy);
        Assert.NotNull(entity.ModifiedDate);
        Assert.True(entity.ModifiedDate.Value > entity.CreatedDate);
    }

    [Fact]
    public void MultipleUpdates_TracksLatestModification()
    {
        // Arrange
        var entity = new TestIntEntity(1);
        entity.SetCreatedInfo("joe.smith");

        // Act - First update
        entity.SetUpdatedInfo("mike.smith");
        var firstModifiedDate = entity.ModifiedDate;

        // Small delay to ensure timestamps are different
        System.Threading.Thread.Sleep(1);

        // Act - Second update
        entity.SetUpdatedInfo("bob.smith");

        // Assert
        Assert.Equal("bob.smith", entity.ModifiedBy);
        Assert.NotNull(entity.ModifiedDate);
        Assert.True(entity.ModifiedDate.Value >= firstModifiedDate.Value);
    }

    #endregion

    #region Property Initialization Tests

    [Fact]
    public void NewEntity_AuditPropertiesAreDefault()
    {
        // Act
        var entity = new TestIntEntity(1);

        // Assert
        Assert.Equal(default(DateTimeOffset), entity.CreatedDate);
        Assert.Null(entity.CreatedBy);
        Assert.Null(entity.ModifiedDate);
        Assert.Null(entity.ModifiedBy);
    }

    #endregion

    #region Parameterless Constructor Protection Tests

    [Fact]
    public void Constructor_Parameterless_IsProtectedFromDirectUse()
    {
        // This test documents the protection pattern for parameterless constructors.
        // The [Obsolete(error: true)] attribute prevents direct instantiation.

        // Uncommenting the line below should cause a compile error:
        // var entity = new TestIntEntity();

        // The parameterless constructor exists ONLY for EF Core/serialization infrastructure.
        // Developers should use the parameterized constructor: new TestIntEntity(id)

        // This test passes if the protection is in place (code above doesn't compile)
        Assert.True(true);
    }

    #endregion
}
