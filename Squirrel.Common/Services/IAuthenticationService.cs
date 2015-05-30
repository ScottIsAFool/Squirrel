using Cimbalino.Phone.Toolkit.Services;
using PocketSharp;
using PocketSharp.Models;

namespace Squirrel.Services
{
    public interface IAuthenticationService
    {
        void StartService(IApplicationSettingsService settings, IPocketClient pocketClient);
        bool IsLoggedIn { get; }
        PocketUser LoggedInUser { get; set; }
        void SetUser(PocketUser user, IPocketClient client);
        void SetAccessToken(IPocketClient client);
        void SignOut();
    }
}