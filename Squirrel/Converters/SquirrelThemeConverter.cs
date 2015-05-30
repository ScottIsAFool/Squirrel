using System;
using System.Globalization;
using System.Windows.Data;
using Squirrel.Model;

namespace Squirrel.Converters
{
    public class SquirrelThemeConverter : IValueConverter
    {
        public object DefaultValue { get; set; }
        public object BrownValue { get; set; }
        public object LightValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var theme = (SquirrelTheme)value;

            switch (theme)
            {
                case SquirrelTheme.Acorn:
                    return BrownValue;
                case SquirrelTheme.Light:
                    return LightValue;
                default:
                    return DefaultValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
