using FluentAssertions;
using MyPOS99.ViewModels;
using Xunit;

namespace MyPOS99.Tests.ViewModels
{
    /// <summary>
    /// Unit tests for ViewModelBase
    /// </summary>
    public class ViewModelBaseTests
    {
        private class TestViewModel : ViewModelBase
        {
            private string _testProperty = string.Empty;

            public string TestProperty
            {
                get => _testProperty;
                set => SetProperty(ref _testProperty, value);
            }
        }

        [Fact]
        public void SetProperty_WhenValueChanges_ShouldRaisePropertyChanged()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(TestViewModel.TestProperty))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.TestProperty = "New Value";

            // Assert
            propertyChangedRaised.Should().BeTrue();
            viewModel.TestProperty.Should().Be("New Value");
        }

        [Fact]
        public void SetProperty_WhenValueDoesNotChange_ShouldNotRaisePropertyChanged()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.TestProperty = "Initial Value";
            var propertyChangedCount = 0;
            viewModel.PropertyChanged += (sender, args) => propertyChangedCount++;

            // Act
            viewModel.TestProperty = "Initial Value"; // Same value

            // Assert
            propertyChangedCount.Should().Be(0);
        }
    }
}
