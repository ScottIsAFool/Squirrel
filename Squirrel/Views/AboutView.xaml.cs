using System.Windows;
using Cimbalino.Phone.Toolkit.Helpers;
using Microsoft.Phone.Tasks;
using ScottIsAFool.WindowsPhone.Logging;
using Squirrel.Resources;

namespace Squirrel.Views
{
    public partial class AboutView 
    {
        public AboutView()
        {
            InitializeComponent();

            var app = ApplicationManifest.Current.App;
            AppName.Text = app.Title;
            VersionNumber.Text = app.Version;
            DescriptionText.Text = AppResources.LabelAppShortDescription;
        }

        private void SupportButton_OnClick(object sender, RoutedEventArgs e)
        {
            new EmailComposeTask
            {
                To = "squirrelapp@outlook.com",
                Subject = "Feedback from Squirrel"
            }.Show();
        }

        private void RateAppButton_OnClick(object sender, RoutedEventArgs e)
        {
            new MarketplaceDetailTask().Show();
        }

        private void SendLogsButton_OnClick(object sender, RoutedEventArgs e)
        {
            new EmailComposeTask
            {
                To = "squirrelapp@outlook.com",
                Subject = string.Format("{0} log file ({1})", ApplicationManifest.Current.App.Title, ApplicationManifest.Current.App.Version),
                Body = WPLogger.GetLogs()
            }.Show();
        }
    }
}
