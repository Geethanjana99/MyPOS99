using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MyPOS99.Converters
{
    public class StringEqualsToVisibilityConverter : IValueConverter
    {
        public string ComparisonValue { get; set; } = string.Empty;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;
            var compareValue = parameter as string ?? ComparisonValue;
            
            return string.Equals(stringValue, compareValue, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
