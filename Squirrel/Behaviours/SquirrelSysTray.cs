using System.Windows;
using System.Windows.Media;
using ScottIsAFool.WindowsPhone.Behaviours;

namespace Squirrel.Behaviours
{
    public class SquirrelSysTray : SystemTrayProgressIndicatorBehaviour
    {
        public SquirrelSysTray()
        {
            DotColor = ((SolidColorBrush) Application.Current.Resources["PhoneAccentBrush"]).Color;
        }
    }
}
