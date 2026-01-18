# Contributing to MyPOS99

Thank you for your interest in contributing to MyPOS99! ??

## ?? Table of Contents
- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Process](#development-process)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Features](#suggesting-features)

## ?? Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on what is best for the community
- Show empathy towards other contributors

## ?? Getting Started

### Prerequisites
- Visual Studio 2022 or later
- .NET 8 SDK or later
- Git
- Basic knowledge of C#, WPF, and MVVM pattern

### Setting Up Development Environment

1. **Fork the repository** on GitHub

2. **Clone your fork**:
   ```bash
   git clone https://github.com/YOUR-USERNAME/MyPOS99.git
   cd MyPOS99
   ```

3. **Add upstream remote**:
   ```bash
   git remote add upstream https://github.com/Geethanjana99/MyPOS99.git
   ```

4. **Install dependencies**:
   ```bash
   dotnet restore
   ```

5. **Build the project**:
   ```bash
   dotnet build
   ```

6. **Run tests**:
   ```bash
   cd MyPOS99.Tests
   dotnet test
   ```

## ?? Development Process

### 1. Create a Branch

```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/bug-description
```

Branch naming conventions:
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation updates
- `test/` - Adding or updating tests
- `refactor/` - Code refactoring

### 2. Make Your Changes

- Write clean, readable code
- Follow existing code style
- Add comments where necessary
- Update documentation if needed

### 3. Test Your Changes

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "ProductServiceTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### 4. Commit Your Changes

Follow conventional commit messages:

```bash
git commit -m "feat: add barcode scanning feature"
git commit -m "fix: resolve database connection issue"
git commit -m "docs: update installation guide"
git commit -m "test: add unit tests for SaleService"
```

Commit types:
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `test:` - Adding or updating tests
- `refactor:` - Code refactoring
- `style:` - Code style changes (formatting)
- `chore:` - Maintenance tasks

### 5. Keep Your Branch Updated

```bash
git fetch upstream
git rebase upstream/master
```

### 6. Push Your Changes

```bash
git push origin feature/your-feature-name
```

## ?? Coding Standards

### C# Conventions

```csharp
// ? Good - Use XML documentation for public APIs
/// <summary>
/// Adds a new product to the database.
/// </summary>
/// <param name="product">The product to add.</param>
/// <returns>The ID of the newly created product.</returns>
public async Task<int> AddProductAsync(Product product)
{
    // Implementation
}

// ? Good - Use meaningful names
var lowStockProducts = await GetLowStockProductsAsync();

// ? Bad - Unclear abbreviations
var lsp = await GetLSPAsync();

// ? Good - Use async/await for database operations
public async Task<List<Product>> GetAllProductsAsync()
{
    return await _db.ExecuteQueryAsync(...);
}

// ? Bad - Blocking calls
public List<Product> GetAllProducts()
{
    return _db.ExecuteQuery(...).Result; // Don't block!
}
```

### MVVM Pattern

```csharp
// ? Good - ViewModels should not access database directly
public class ProductViewModel : ViewModelBase
{
    private readonly ProductService _productService;
    
    public ProductViewModel(ProductService productService)
    {
        _productService = productService;
    }
}

// ? Bad - Don't access database from ViewModel
public class ProductViewModel : ViewModelBase
{
    private readonly DatabaseContext _db; // Bad!
}
```

### Naming Conventions

- **Classes**: PascalCase (e.g., `ProductService`)
- **Methods**: PascalCase (e.g., `GetAllProductsAsync`)
- **Private fields**: _camelCase (e.g., `_productService`)
- **Properties**: PascalCase (e.g., `ProductName`)
- **Local variables**: camelCase (e.g., `productList`)
- **Constants**: PascalCase (e.g., `MaxRetryCount`)

## ?? Testing Guidelines

### Writing Tests

```csharp
[Fact]
public async Task MethodName_Condition_ExpectedBehavior()
{
    // Arrange - Set up test data
    var product = new Product { Code = "TEST001" };
    
    // Act - Perform the action
    var result = await _productService.AddProductAsync(product);
    
    // Assert - Verify the result
    result.Should().BeGreaterThan(0);
}
```

### Test Coverage Requirements

- All new features must include unit tests
- Aim for at least 80% code coverage
- Test both success and failure scenarios
- Test edge cases (null, empty, boundary values)

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests in specific file
dotnet test --filter "ProductServiceTests"

# Run with detailed output
dotnet test --verbosity detailed

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## ?? Pull Request Process

### Before Submitting

- [ ] Code follows the project's style guidelines
- [ ] All tests pass
- [ ] New tests added for new features
- [ ] Documentation updated (README, XML comments)
- [ ] No merge conflicts
- [ ] Commits are meaningful and follow conventions

### Creating Pull Request

1. **Push your branch** to your fork
2. **Create PR** on GitHub
3. **Fill out the PR template**:
   - Description of changes
   - Related issue (if any)
   - Testing performed
   - Screenshots (if UI changes)

### PR Template

```markdown
## Description
Brief description of what this PR does

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Related Issue
Fixes #(issue number)

## Testing
- [ ] Unit tests added
- [ ] Manual testing performed
- [ ] All tests pass

## Screenshots (if applicable)
Add screenshots here

## Checklist
- [ ] Code follows style guidelines
- [ ] Documentation updated
- [ ] Tests added and passing
- [ ] No breaking changes
```

### Review Process

1. Maintainer will review your PR
2. Address any requested changes
3. Once approved, PR will be merged
4. Your contribution will be credited!

## ?? Reporting Bugs

### Before Submitting a Bug Report

1. **Check existing issues** - Your bug might already be reported
2. **Use latest version** - Bug might be fixed already
3. **Reproduce the bug** - Ensure it's consistently reproducible

### Bug Report Template

```markdown
**Describe the Bug**
Clear description of what the bug is

**To Reproduce**
Steps to reproduce:
1. Go to '...'
2. Click on '...'
3. See error

**Expected Behavior**
What you expected to happen

**Screenshots**
If applicable, add screenshots

**Environment:**
 - OS: [e.g., Windows 11]
 - .NET Version: [e.g., 8.0]
 - Application Version: [e.g., 1.0.0]

**Additional Context**
Any other relevant information
```

## ?? Suggesting Features

### Feature Request Template

```markdown
**Feature Description**
Clear description of the feature

**Problem it Solves**
What problem does this feature address?

**Proposed Solution**
How you envision this working

**Alternatives Considered**
Other solutions you've thought about

**Additional Context**
Mockups, examples, or references
```

## ?? Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [WPF Documentation](https://docs.microsoft.com/dotnet/desktop/wpf/)
- [MVVM Pattern](https://docs.microsoft.com/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern)
- [xUnit Documentation](https://xunit.net/)

## ?? Good First Issues

Look for issues labeled `good first issue` - these are great starting points for new contributors!

## ?? Thank You!

Your contributions make this project better for everyone. Thank you for taking the time to contribute! ??

---

**Questions?** Feel free to ask in [GitHub Discussions](https://github.com/Geethanjana99/MyPOS99/discussions)
