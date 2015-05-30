using System;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using Cimbalino.Phone.Toolkit.Extensions;
using Facebook.Client;
using GalaSoft.MvvmLight.Messaging;
using PocketWP;
using ScottIsAFool.WindowsPhone.Extensions;
using Squirrel.Services;

namespace Squirrel
{
    public class SquirrelUriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
            // if URI is a facebook login response, handle the deep link 
            if (AppAuthenticationHelper.IsFacebookLoginResponse(uri))
            {
                var session = new FacebookSession();

                try
                {
                    session.ParseQueryString(HttpUtility.UrlDecode(uri.ToString()));
                    
                    SessionStorage.Save(session);
                    
                    return new Uri(Constants.Pages.Login.AuthorisingView + "?authtype=facebook", UriKind.Relative);
                }
                catch (Facebook.FacebookOAuthException exc)
                {
                        // Handle error case
                        MessageBox.Show("Not signed in: " + exc.Message);
                }

                uri = new Uri(Constants.Pages.MainPage, UriKind.Relative);
            }

            if (uri.ToString().Contains("PocketAuthorise"))
            {
                // /Protocol?encodedLaunchUri=squirrel%3APocketAuthorise%3FReturnUri%3D%2FViews%5CMainPage.xaml
                var squirrelUrl = Uri.UnescapeDataString(uri.QueryString()["encodedLaunchUri"]);
                return new Uri(Constants.Pages.Login.AuthorisingView, UriKind.Relative);
            }

            if (PocketHelper.HasPocketData(uri))
            {
                var item = PocketHelper.RetrievePocketData(uri);

                if (item.Items.IsNullOrEmpty())
                {
                    MessageBox.Show("Sorry, no items were sent to add. Sorry for the inconvenience", "Unsupported request", MessageBoxButton.OK);
                    if (!string.IsNullOrEmpty(item.CallbackUri))
                    {
                        Windows.System.Launcher.LaunchUriAsync(new Uri(item.CallbackUri, UriKind.Absolute));
                        Application.Current.Terminate();
                    }
                    else
                    {
                        return new Uri(Constants.Pages.MainPage, UriKind.Relative);
                    }
                }
                else if (item.Items.Count == 1)
                {
                    Messenger.Default.Send(new NotificationMessage(item, Constants.Messages.DetailsFromExternalMsg));
                    return new Uri(Constants.Pages.AdvancedAddView + "?clearbackstack=true", UriKind.Relative);
                }
                else
                {
                    MessageBox.Show("Sorry, adding multiple items is not supported at this time. Sorry for the inconvenience", "Unsupported request", MessageBoxButton.OK);
                    if (!string.IsNullOrEmpty(item.CallbackUri))
                    {
                        Windows.System.Launcher.LaunchUriAsync(new Uri(item.CallbackUri, UriKind.Absolute));
                        Application.Current.Terminate();
                    }
                    else
                    {
                        return new Uri(Constants.Pages.MainPage, UriKind.Relative);
                    }
                }
            }

            if (!AuthenticationService.Current.IsLoggedIn && NotLoginPage(uri))
            {
                var loginUrl = string.Format(Constants.Pages.Login.LoginView + "&clearbackstack=true", Constants.Pages.FirstRun.OfflineView);
                return new Uri(loginUrl, UriKind.Relative);
            }

            return uri;
        }

        private static bool NotLoginPage(Uri uri)
        {
            return !uri.ToString().Contains(Constants.Pages.Login.AuthorisingView) &&
                   !uri.ToString().Contains(Constants.Pages.Login.NewUserView);
        }
    }
}
