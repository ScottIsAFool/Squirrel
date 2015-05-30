using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Cimbalino.Phone.Toolkit.Services;
using Facebook.Client;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using PocketSharp;
using ScottIsAFool.WindowsPhone.ViewModel;
using Squirrel.Extensions;
using Squirrel.Model;
using Squirrel.Resources;
using Squirrel.Services;
using INavigationService = Squirrel.Services.INavigationService;

namespace Squirrel.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private IPocketClient _pocketClient;

        private bool _isFirstLoad;
        private AuthType _authType;

        /// <summary>
        /// Initializes a new instance of the LoginViewModel class.
        /// </summary>
        public LoginViewModel(INavigationService navigationService, IPocketClient pocketClient)
        {
            _navigationService = navigationService;
            _pocketClient = pocketClient;
        }

        public string ReturnUri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string AlmostDone { get; set; }

        public bool CanCreateUser
        {
            get
            {
                return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(EmailAddress);
            }
        }

        public RelayCommand ConnectWithPocketCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (!await _navigationService.IsNetworkAvailable())
                    {
                        return;
                    }

                    _isFirstLoad = true;
                    _navigationService.NavigateTo(Constants.Pages.Login.AuthorisingView);
                });
            }
        }

        public RelayCommand AuthorisingViewLoaded
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    switch (_authType)
                    {
                        case AuthType.Pocket:
                            await PocketAuthentication();
                            break;
                        case AuthType.Facebook:
                            await FacebookAuthentication();
                            break;
                    }
                });
            }
        }

        private async Task FacebookAuthentication()
        {
            SetProgressBar(AppResources.SysTrayTalkingToFacebook);

            var session = SessionStorage.Load();

            if (!string.IsNullOrEmpty(session.AccessToken))
            {
                var now = DateTime.Now;
                if (now < session.Expires)
                {
                    var facebookUrl = string.Format(Constants.FacebookApiUrl, session.AccessToken);

                    var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });

                    var response = await client.GetAsync(new Uri(facebookUrl, UriKind.Absolute), HttpCompletionOption.ResponseContentRead);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var user = await JsonConvert.DeserializeObjectAsync<FacebookUser>(json);

                        Username = user.Username;
                        EmailAddress = user.Email;
                        AlmostDone = string.Format(AppResources.LabelAlmostDone, user.FirstName);

                        _navigationService.NavigateTo(Constants.Pages.Login.NewUserView);
                    }
                }
                else
                {
                    MessageBox.Show(AppResources.ErrorFacebookToken);
                    _navigationService.NavigateTo(Constants.Pages.MainPage);
                }
            }

            SetProgressBar();
        }

        private async Task PocketAuthentication()
        {
            if (_isFirstLoad)
            {
                if (string.IsNullOrEmpty(ReturnUri))
                {
                    ReturnUri = Constants.Pages.MainPage;
                }

                SetProgressBar(AppResources.SysTrayTalkingToPocket);

                var callbackUri = string.Format(Constants.CallBackUri, Uri.EscapeUriString(ReturnUri));
                _pocketClient = new PocketClient(Constants.PocketConsumerKey, callbackUri: callbackUri);

                try
                {
                    await _pocketClient.GetRequestCode();

                    SetProgressBar(AppResources.SysTraySendingToPocket);
                    var authUri = _pocketClient.GenerateAuthenticationUri();

                    _isFirstLoad = false;

                    new WebBrowserService().Show(authUri);
                    SetProgressBar();
                }
                catch (PocketException ex)
                {
                    ex.Log("PocketAuthentication() - Generating auth url", Log);
                }
                catch (Exception ex)
                {
                    Log.ErrorException("ConnectWithPocketCommand - Request Code", ex);
                }
            }
            else
            {
                try
                {
                    SetProgressBar(AppResources.SysTrayGettingAccessToken);
                    var user = await _pocketClient.GetUser();
                    AuthenticationService.Current.SetUser(user, _pocketClient);
                    CacheService.Current.CreateCacheForUser();

                    SetProgressBar();

                    _navigationService.NavigateTo(ReturnUri + "?clearbackstack=true");
                }
                catch (PocketException ex)
                {
                    ex.Log("PocketAuthentication() - Getting Access Token", Log);
                }
                catch (Exception ex)
                {
                    Log.ErrorException("ConnectWithPocketCommand - Access Token", ex);
                }
            }
        }

        public RelayCommand SignUpWithFacebookCommand
        {
            get
            {
                return new RelayCommand(() => new FacebookSessionClient(Constants.FacebookAppId).LoginWithApp("basic_info,email"));
            }
        }

        public RelayCommand CreateUserCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (!CanCreateUser)
                    {
                        return;
                    }

                    try
                    {
                        SetProgressBar(AppResources.SysTrayRegisteringNewAccount);

                        var result = await _pocketClient.RegisterAccount(Username, EmailAddress, Password);

                        if (result)
                        {
                            Username = Password = EmailAddress = string.Empty;
                        }

                        var message = result
                            ? AppResources.InfoSuccessfulAccount
                            : AppResources.ErrorAccountFailure;

                        SetProgressBar();
                        MessageBox.Show(message, AppResources.MessageAccountTitle, MessageBoxButton.OK);

                        if (result)
                        {
                            _navigationService.GoBack();
                        }
                    }
                    catch (Exception ex)
                    {
                        SetProgressBar();
                        Log.ErrorException("CreateUserCommand", ex);
                        MessageBox.Show(AppResources.ErrorCreatingAccount, AppResources.ErrorTitle, MessageBoxButton.OK);
                    }
                });
            }
        }

        public override void WireMessages()
        {
            Messenger.Default.Register<NotificationMessage>(this, m =>
            {
                if (m.Notification.Equals(Constants.Messages.AuthTypeMsg))
                {
                    _authType = (AuthType)m.Sender;
                }
            });
        }
    }
}