using System.Threading.Tasks;

namespace Squirrel.Services
{
    public interface INavigationService : Cimbalino.Phone.Toolkit.Services.INavigationService
    {
        Task<bool> IsNetworkAvailable();
    }
}
