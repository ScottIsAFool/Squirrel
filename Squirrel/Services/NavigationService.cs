using System.Threading.Tasks;
using System.Windows;

namespace Squirrel.Services
{
    public class NavigationService : Cimbalino.Phone.Toolkit.Services.NavigationService, INavigationService
    {
        public async Task<bool> IsNetworkAvailable()
        {
            return await NavigationUtils.CheckNetwork(action: () =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    App.ShowMessage("No network connection available");
                });
            });
        }
    }
}
