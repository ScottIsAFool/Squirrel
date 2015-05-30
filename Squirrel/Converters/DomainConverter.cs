using System;
using System.Globalization;
using System.Windows.Data;

namespace Squirrel.Converters
{
    public class DomainConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            Uri uri;
            if (value is string)
            {
                if (!Uri.TryCreate(value.ToString(), UriKind.Absolute, out uri))
                {
                    return string.Empty;
                }
            }

            if (value is Uri)
            {
                uri = (Uri) value;
            }
            else
            {
                return string.Empty;
            }

            return uri.Host.Replace("www.", string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FavIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            Uri uri;
            if (value is string)
            {
                if (!Uri.TryCreate(value.ToString(), UriKind.Absolute, out uri))
                {
                    return string.Empty;
                }
            }

            if (value is Uri)
            {
                uri = (Uri)value;
            }
            else
            {
                return string.Empty;
            }

            return string.Format("http://www.google.com/s2/favicons?domain={0}://{1}", uri.Scheme, uri.Host);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
