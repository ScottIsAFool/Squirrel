using System;
using System.Globalization;
using Cimbalino.Phone.Toolkit.Converters;

namespace Squirrel.Converters
{
    public class ImageUrlConverter : MultiValueConverterBase
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object[] ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
