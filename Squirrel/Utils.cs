using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Squirrel
{
    public static class Utils
    {
        internal static void CopyItem<T>(T source, T destination) where T : class
        {
            foreach (var sourcePropertyInfo in source.GetType().GetProperties())
            {
                var destPropertyInfo = source.GetType().GetProperty(sourcePropertyInfo.Name);

                if (sourcePropertyInfo.CanWrite)
                {
                    destPropertyInfo.SetValue(
                    destination,
                    sourcePropertyInfo.GetValue(source, null),
                    null);
                }
            }
        }

        internal static CheckBox CreateDontShowCheckBox(string propertyName)
        {
            var cb = new CheckBox { Content = "Don't show this message again" };

            var binding = new Binding
            {
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(propertyName),
                Source = App.Settings
            };

            cb.SetBinding(CheckBox.IsCheckedProperty, binding);

            return cb;
        }
    }
}
