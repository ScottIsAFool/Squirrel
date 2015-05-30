using System;
using System.Windows;
using Microsoft.Phone.Controls;
using PocketWP;

namespace SquirrelTestApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //PocketHelper.AddItemToPocket("http://metronuggets.com", title: "Metro Nuggets");

            Windows.System.Launcher.LaunchUriAsync(new System.Uri("pocket:Add?Url=" + Uri.EscapeDataString("http://metronuggets.com") + "&source=fromweb"));
        }
    }
}