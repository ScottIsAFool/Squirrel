using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Squirrel.Converters
{
    public class BoolToStringConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty TrueValueProperty =
            DependencyProperty.Register("TrueValue", typeof (string), typeof (BoolToStringConverter), new PropertyMetadata(default(string)));

        public string TrueValue
        {
            get { return (string) GetValue(TrueValueProperty); }
            set { SetValue(TrueValueProperty, value); }
        }

        public static readonly DependencyProperty FalseValueProperty =
            DependencyProperty.Register("FalseValue", typeof (string), typeof (BoolToStringConverter), new PropertyMetadata(default(string)));

        public string FalseValue
        {
            get { return (string) GetValue(FalseValueProperty); }
            set { SetValue(FalseValueProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
