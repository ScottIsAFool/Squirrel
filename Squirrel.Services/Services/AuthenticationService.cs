using Cimbalino.Phone.Toolkit.Services;
using PocketSharp;
using PocketSharp.Models;
using PropertyChanged;

namespace Squirrel.Services
{
    [ImplementPropertyChanged]
    public class AuthenticationService : IAuthenticationService
    {
        private static AuthenticationService _current;

        public static AuthenticationService Current { get { return _current ?? (_current = new AuthenticationService()); } }

        private IApplicationSettingsService _settings;

        public void StartService(IApplicationSettingsService settings, IPocketClient pocketClient)
        {
            _settings = settings;
            LoggedInUser = GetUser();
            SetAccessToken(pocketClient);
        }

        public bool IsLoggedIn { get { return LoggedInUser != null; } }
        public PocketUser LoggedInUser { get; set; }

        private PocketUser GetUser()
        {
            return _settings.Get<PocketUser>(Constants.StorageSettings.AccessToken, null);
        }

        public void SetUser(PocketUser user, IPocketClient client)
        {
            LoggedInUser = user;
            client.AccessCode = LoggedInUser.Code;

            _settings.Set(Constants.StorageSettings.AccessToken, LoggedInUser);
            _settings.Save();
        }

        public void SetAccessToken(IPocketClient client)
        {
            if (LoggedInUser != null && !string.IsNullOrEmpty(LoggedInUser.Code))
            {
                client.AccessCode = LoggedInUser.Code;
            }
        }

        public void SignOut()
        {
            LoggedInUser = null;
            _settings.Reset(Constants.StorageSettings.AccessToken);
            _settings.Save();
        }
    }
}
