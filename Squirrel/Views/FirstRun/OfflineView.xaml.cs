using System;
using System.Windows;

namespace Squirrel.Views.FirstRun
{
    public partial class OfflineView
    {
        // Constructor
        public OfflineView()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri(Constants.Pages.MainPage + "?clearbackstack=true", UriKind.Relative));
        }
    }
}