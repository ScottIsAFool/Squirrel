using System.Windows;
using System.Windows.Media;
using Cimbalino.Phone.Toolkit.Behaviors;
using ApplicationBar = Cimbalino.Phone.Toolkit.Behaviors.ApplicationBar;

namespace Squirrel.Behaviours
{
    public class SquirrelApplicationBarBehaviour : ApplicationBarBehavior
    {
        public SquirrelApplicationBarBehaviour()
        {
            BackgroundColor = (Color)Application.Current.Resources["SquirrelAltBackgroundColor"];
            Opacity = 0.85;
            ForegroundColor = (Color)Application.Current.Resources["SquirrelBackgroundColor"];

            StateChanged += (sender, args) =>
            {
                Opacity = args.IsMenuVisible ? 1 : 0.85;
            };
        }
    }

    public class SquirrelApplicationBar : ApplicationBar
    {
        public SquirrelApplicationBar()
        {
            BackgroundColor = (Color)Application.Current.Resources["SquirrelAltBackgroundColor"];
            Opacity = 0.85;
            ForegroundColor = (Color)Application.Current.Resources["SquirrelBackgroundColor"];

            StateChanged += (sender, args) =>
            {
                Opacity = args.IsMenuVisible ? 1 : 0.85;
            };
        }
    }
}
