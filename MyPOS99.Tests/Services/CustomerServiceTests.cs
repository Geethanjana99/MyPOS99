using FluentAssertions;
using Moq;
using MyPOS99.Data;
using MyPOS99.Models;
using MyPOS99.Services;
using Xunit;

namespace MyPOS99.Tests.Services
{
    /// <summary>
    /// Unit tests for CustomerService
    /// </summary>
    public class CustomerServiceTests
    {
        private readonly Mock<DatabaseContext> _mockDb;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _mockDb = new Mock<DatabaseContext>();
            _customerService = new CustomerService(_mockDb.Object);
        }

        [Fact]
        public async Task AddCustomerAsync_WithValidCustomer_ShouldSucceed()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "John Doe",
                Phone = "1234567890",
                Email = "john@example.com",
                Address = "123 Main St"
            };

            // Act
            var result = await _customerService.AddCustomerAsync(customer);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllCustomersAsync_ShouldReturnCustomers()
        {
            // Act
            var result = await _customerService.GetAllCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<Customer>>();
        }

        [Theory]
        [InlineData("john@example.com", true)]
        [InlineData("invalid-email", false)]
        [InlineData("", false)]
        public void ValidateEmail_ShouldReturnCorrectResult(string email, bool expectedValid)
        {
            // Act
            var isValid = IsValidEmail(email);

            // Assert
            isValid.Should().Be(expectedValid);
        }

        [Fact]
        public async Task SearchCustomersAsync_WithValidTerm_ShouldReturnMatches()
        {
            // Arrange
            var searchTerm = "John";

            // Act
            var result = await _customerService.SearchCustomersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<Customer>>();
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
