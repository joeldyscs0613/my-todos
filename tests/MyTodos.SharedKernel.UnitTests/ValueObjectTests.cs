using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.SharedKernel.UnitTests;

public class ValueObjectTests
{
    #region Test Value Object Classes

    private class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string? ZipCode { get; }

        public Address(string street, string city, string? zipCode = null)
        {
            Street = street;
            City = city;
            ZipCode = zipCode;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return ZipCode;
        }
    }

    private class Email : ValueObject
    {
        public string Value { get; }

        public Email(string value)
        {
            Value = value;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    private class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    #endregion

    #region Equals Tests

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "12345");
        var address2 = new Address("123 Main St", "Springfield", "12345");

        // Act
        var result = address1.Equals(address2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "12345");
        var address2 = new Address("456 Oak Ave", "Springfield", "12345");

        // Act
        var result = address1.Equals(address2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithNullValue_ReturnsFalse()
    {
        // Arrange
        var address = new Address("123 Main St", "Springfield", "12345");

        // Act
        var result = address.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var address = new Address("123 Main St", "Springfield", "12345");
        var email = new Email("test@example.com");

        // Act
        var result = address.Equals(email);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithNullComponentInBoth_ReturnsTrue()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", null);
        var address2 = new Address("123 Main St", "Springfield", null);

        // Act
        var result = address1.Equals(address2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithNullComponentInOne_ReturnsFalse()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "12345");
        var address2 = new Address("123 Main St", "Springfield", null);

        // Act
        var result = address1.Equals(address2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithSameReference_ReturnsTrue()
    {
        // Arrange
        var address = new Address("123 Main St", "Springfield", "12345");

        // Act
        var result = address.Equals(address);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHashCode()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "12345");
        var address2 = new Address("123 Main St", "Springfield", "12345");

        // Act
        var hash1 = address1.GetHashCode();
        var hash2 = address2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ReturnsDifferentHashCode()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "12345");
        var address2 = new Address("456 Oak Ave", "Springfield", "12345");

        // Act
        var hash1 = address1.GetHashCode();
        var hash2 = address2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_WithNullComponents_DoesNotThrow()
    {
        // Arrange
        var address = new Address("123 Main St", "Springfield", null);

        // Act & Assert - Should not throw
        var hash = address.GetHashCode();
        Assert.NotEqual(0, hash); // Hash code should be calculated even with null
    }

    [Fact]
    public void GetHashCode_CalledMultipleTimes_ReturnsSameValue()
    {
        // Arrange
        var address = new Address("123 Main St", "Springfield", "12345");

        // Act
        var hash1 = address.GetHashCode();
        var hash2 = address.GetHashCode();
        var hash3 = address.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(hash2, hash3);
    }

    #endregion

    #region Operator == Tests

    [Fact]
    public void OperatorEquals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(100.50m, "USD");

        // Act
        var result = money1 == money2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEquals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(200.00m, "USD");

        // Act
        var result = money1 == money2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEquals_WithBothNull_ReturnsTrue()
    {
        // Arrange
        Address? address1 = null;
        Address? address2 = null;

        // Act
        var result = address1 == address2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEquals_WithOneNull_ReturnsFalse()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "12345");
        Address? address2 = null;

        // Act
        var result1 = address1 == address2;
        var result2 = address2 == address1;

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void OperatorEquals_WithSameReference_ReturnsTrue()
    {
        // Arrange
        var address = new Address("123 Main St", "Springfield", "12345");
        var sameAddress = address;

        // Act
        var result = address == sameAddress;

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Operator != Tests

    [Fact]
    public void OperatorNotEquals_WithSameValues_ReturnsFalse()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act
        var result = email1 != email2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEquals_WithDifferentValues_ReturnsTrue()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("other@example.com");

        // Act
        var result = email1 != email2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorNotEquals_WithBothNull_ReturnsFalse()
    {
        // Arrange
        Email? email1 = null;
        Email? email2 = null;

        // Act
        var result = email1 != email2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEquals_WithOneNull_ReturnsTrue()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        Email? email2 = null;

        // Act
        var result1 = email1 != email2;
        var result2 = email2 != email1;

        // Assert
        Assert.True(result1);
        Assert.True(result2);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void ValueObject_WithMultipleComponents_ComparesAllComponents()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "12345");
        var address2 = new Address("123 Main St", "Springfield", "54321");

        // Act
        var result = address1 == address2;

        // Assert
        // Should be false because ZipCode is different
        Assert.False(result);
    }

    [Fact]
    public void ValueObject_UsedInCollections_WorksCorrectly()
    {
        // Arrange
        var addresses = new HashSet<Address>
        {
            new Address("123 Main St", "Springfield", "12345"),
            new Address("123 Main St", "Springfield", "12345"), // Duplicate
            new Address("456 Oak Ave", "Springfield", "12345")
        };

        // Assert
        // HashSet should contain only 2 unique addresses
        Assert.Equal(2, addresses.Count);
    }

    [Fact]
    public void ValueObject_UsedAsDictionaryKey_WorksCorrectly()
    {
        // Arrange
        var dictionary = new Dictionary<Email, string>();
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act
        dictionary[email1] = "User 1";
        dictionary[email2] = "User 2"; // Should overwrite because email1 == email2

        // Assert
        Assert.Single(dictionary);
        Assert.Equal("User 2", dictionary[email1]);
    }

    [Fact]
    public void ValueObject_WithDecimalComponent_HandlesEqualityCorrectly()
    {
        // Arrange
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(100.50m, "USD");
        var money3 = new Money(100.51m, "USD");

        // Act & Assert
        Assert.Equal(money1, money2);
        Assert.NotEqual(money1, money3);
    }

    #endregion
}
