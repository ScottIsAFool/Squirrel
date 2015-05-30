using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Phone.Controls;
using PocketSharp.Models;
using Telerik.Windows.Controls;

namespace Squirrel.Controls
{
    public class TagWindow : RadWindow
    {
        public TagWindow()
        {
            OpenAnimation = new RadMoveYAnimation { Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300)), EndY = 0.0, StartY = 800.0 };
            CloseAnimation = new RadMoveYAnimation { Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300)), EndY = 800.0, StartY = 0.0 };
            WindowOpening += OnWindowOpened;
            WindowClosed += OnWindowClosed;
        }

        private void OnWindowClosed(object sender, WindowClosedEventArgs windowClosedEventArgs)
        {
            var currentPage = App.RootFrame.Content as PhoneApplicationPage;
            if (currentPage != null && currentPage.ApplicationBar != null)
            {
                currentPage.ApplicationBar.IsVisible = true;
            }
        }

        private void OnWindowOpened(object sender, EventArgs eventArgs)
        {
            var currentPage = App.RootFrame.Content as PhoneApplicationPage;
            if (currentPage != null && currentPage.ApplicationBar != null)
            {
                currentPage.ApplicationBar.IsVisible = false;
            }
        }
    }
}
