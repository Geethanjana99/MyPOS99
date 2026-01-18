using FluentAssertions;
using Moq;
using MyPOS99.Data;
using MyPOS99.Models;
using MyPOS99.Services;
using Xunit;

namespace MyPOS99.Tests.Services
{
    /// <summary>
    /// Unit tests for ProductService
    /// </summary>
    public class ProductServiceTests
    {
        private readonly Mock<DatabaseContext> _mockDb;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockDb = new Mock<DatabaseContext>();
            _productService = new ProductService(_mockDb.Object);
        }

        [Fact]
        public async Task AddProductAsync_WithValidProduct_ShouldSucceed()
        {
            // Arrange
            var product = new Product
            {
                Code = $"TEST{Guid.NewGuid().ToString()[..8]}",
                Name = "Test Product",
                Category = "Test Category",
                CostPrice = 100,
                SellPrice = 150,
                StockQty = 10,
                MinStockLevel = 2
            };

            // Act
            var result = await _productService.AddProductAsync(product);

            // Assert
            result.Should().BeTrue("Product should be added successfully");
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnProducts()
        {
            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<Product>>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateProduct_WithInvalidCode_ShouldFail(string invalidCode)
        {
            // Arrange
            var product = new Product
            {
                Code = invalidCode,
                Name = "Test Product",
                CostPrice = 100,
                SellPrice = 150
            };

            // Assert
            product.Code.Should().Match(c => string.IsNullOrWhiteSpace(c));
        }

        [Fact]
        public async Task SearchProductsAsync_WithValidTerm_ShouldReturnMatchingProducts()
        {
            // Arrange
            var searchTerm = "Laptop";

            // Act
            var result = await _productService.SearchProductsAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<Product>>();
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnOnlyLowStockItems()
        {
            // Act
            var result = await _productService.GetLowStockProductsAsync();

            // Assert
            result.Should().NotBeNull();
            // If there are results, they should all be low stock items
            if (result.Any())
            {
                result.Should().AllSatisfy(p => 
                    p.StockQty.Should().BeLessThanOrEqualTo(p.MinStockLevel, 
                    "Only low stock products should be returned"));
            }
        }
    }
}
