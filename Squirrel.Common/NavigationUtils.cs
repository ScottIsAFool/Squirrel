using System;
using System.Threading.Tasks;
using Microsoft.Phone.Net.NetworkInformation;

namespace Squirrel
{
    public static class NavigationUtils
    {
        public static async Task<bool> CheckNetwork(bool checkIfOnWifi = false, Action action = null)
        {
            var result = false;
            await Task.Run(() =>
            {
                result = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                if (!result || NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.None)
                {
                    if (action != null)
                    {
                        action();
                    }
                }

                if (checkIfOnWifi)
                {
                    if (NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                        NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.VeryHighSpeedDsl &&
                        NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.GigabitEthernet &&
                        NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                    {
                        result = false;
                    }
                }
            });

            return result;
        }
    }
}
