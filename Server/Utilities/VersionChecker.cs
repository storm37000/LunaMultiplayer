using LmpCommon;
using Server.Context;
using Server.Log;
using System;
using System.Threading.Tasks;

namespace Server.Utilities
{
    public class VersionChecker
    {
        private static Version LatestVersion { get; set; }

        public static async void RefreshLatestVersion()
        {
            while (ServerContext.ServerRunning)
            {
                LatestVersion = LmpUpdater.storm.UpdateChecker.GetLatestVersion();

                //Sleep for 30 minutes...
                await Task.Delay(30 * 60 * 1000);
            }
        }

        public static async void DisplayNewVersionMsg()
        {
            while (ServerContext.ServerRunning)
            {
                if (LatestVersion > LmpVersioning.CurrentVersion)
                {
                    LunaLog.Warning($"There is a new version of LMP! Current: {LmpVersioning.CurrentVersion} Latest: {LatestVersion} Please download the one for your OS from https://github.com/storm37000/LunaMultiplayer/releases");
                    if (LmpVersioning.IsCompatible(LatestVersion))
                    {
                        LunaLog.Debug("Your version is compatible with the latest version so you will still be listed in the master servers.");
                    }
                    else
                    {
                        LunaLog.Warning("Your version is NOT compatible with the latest version. You won't be listed in the master servers!");
                    }
                }

                //Sleep for 30 seconds...
                await Task.Delay(30 * 1000);
            }
        }
    }
}
