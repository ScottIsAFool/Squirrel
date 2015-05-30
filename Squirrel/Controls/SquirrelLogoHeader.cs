using System.Windows;
using System.Windows.Controls;

namespace Squirrel.Controls
{
    public class SquirrelLogoHeader : Control
    {
        public SquirrelLogoHeader()
        {
            DefaultStyleKey = typeof (SquirrelLogoHeader);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (SquirrelLogoHeader), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }
}
