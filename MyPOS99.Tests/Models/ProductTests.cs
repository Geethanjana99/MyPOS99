using FluentAssertions;
using MyPOS99.Models;
using Xunit;

namespace MyPOS99.Tests.Models
{
    /// <summary>
    /// Unit tests for Product model
    /// </summary>
    public class ProductTests
    {
        [Fact]
        public void Product_CalculateProfitMargin_ShouldReturnCorrectPercentage()
        {
            // Arrange
            var product = new Product
            {
                CostPrice = 100,
                SellPrice = 150
            };

            // Act
            var profitMargin = ((product.SellPrice - product.CostPrice) / product.CostPrice) * 100;

            // Assert
            profitMargin.Should().Be(50m);
        }

        [Fact]
        public void Product_IsLowStock_ShouldReturnTrueWhenStockBelowMinimum()
        {
            // Arrange
            var product = new Product
            {
                StockQty = 5,
                MinStockLevel = 10
            };

            // Act
            var isLowStock = product.StockQty <= product.MinStockLevel;

            // Assert
            isLowStock.Should().BeTrue();
        }

        [Fact]
        public void Product_TotalValue_ShouldCalculateCorrectly()
        {
            // Arrange
            var product = new Product
            {
                CostPrice = 100,
                StockQty = 10
            };

            // Act
            var totalValue = product.CostPrice * product.StockQty;

            // Assert
            totalValue.Should().Be(1000m);
        }
    }
}
