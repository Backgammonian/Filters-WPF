using System;
using System.Globalization;
using System.Windows.Data;

namespace FiltersWPF
{
    public class PathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = (string)value;
            return (path == null || path == string.Empty) ? "No image selected" : path;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
