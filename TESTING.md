# Testing Guide

## ?? Running Unit Tests

This project includes comprehensive unit tests to ensure code quality and reliability.

## Prerequisites

- Visual Studio 2022 or later
- .NET 8 SDK
- xUnit Test Explorer extension (usually included)

## Running Tests in Visual Studio

### Method 1: Using Test Explorer

1. **Open** Visual Studio
2. **Go to** `Test` ? `Test Explorer` (or press `Ctrl+E, T`)
3. **Click** "Run All Tests" button (??)
4. **View** results in the Test Explorer window

### Method 2: Using Command Line

```powershell
# Navigate to test project
cd MyPOS99.Tests

# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Structure

```
MyPOS99.Tests/
??? Services/
?   ??? ProductServiceTests.cs
?   ??? CustomerServiceTests.cs
??? ViewModels/
?   ??? ViewModelBaseTests.cs
??? Models/
    ??? ProductTests.cs
```

## Test Coverage

Current test coverage includes:

- ? **ProductService**: CRUD operations, search, low stock detection
- ? **CustomerService**: Customer management, email validation
- ? **ViewModelBase**: Property change notifications
- ? **Product Model**: Business logic calculations

## Writing New Tests

### Example Test Structure

```csharp
using FluentAssertions;
using Xunit;

namespace MyPOS99.Tests.Services
{
    public class MyServiceTests
    {
        [Fact]
        public void MyMethod_WithValidInput_ShouldReturnExpectedResult()
        {
            // Arrange
            var input = "test";
            var expected = "expected result";
            
            // Act
            var result = MyMethod(input);
            
            // Assert
            result.Should().Be(expected);
        }
    }
}
```

## Test Frameworks Used

- **xUnit**: Testing framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Readable assertion library

## Continuous Integration

Tests automatically run on every push to GitHub via GitHub Actions.

See build status: ![Build Status](https://github.com/Geethanjana99/MyPOS99/workflows/Build%20and%20Test/badge.svg)

## Adding More Tests

To add tests for a new service:

1. Create a new file in `MyPOS99.Tests/Services/`
2. Follow the naming convention: `{ServiceName}Tests.cs`
3. Write test methods using `[Fact]` or `[Theory]` attributes
4. Use FluentAssertions for readable test assertions

## Best Practices

1. **AAA Pattern**: Arrange, Act, Assert
2. **One assertion per test** (when possible)
3. **Descriptive test names**: `MethodName_Condition_ExpectedBehavior`
4. **Test edge cases**: null values, empty strings, boundary conditions
5. **Mock external dependencies**: databases, file systems, network calls

---

**Happy Testing! ??**
