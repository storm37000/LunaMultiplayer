using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.System
{
    /// <summary>
    /// It's a bad idea to run the GC, but somehow LMP is leaking somewhere. Until the leak is found, this can solve most issues...
    /// </summary>
    public class GcSystem
    {
        public static async void PerformGarbageCollection()
        {
            while (ServerContext.ServerRunning && IntervalSettings.SettingsStore.GcMinutesInterval != 0)
            {
                if (ServerContext.PlayerCount > 0)
                {
                    PerformGCNow();
                }
                await Task.Delay((int)TimeSpan.FromMinutes(IntervalSettings.SettingsStore.GcMinutesInterval).TotalMilliseconds);
            }
        }
        public static void PerformGCNow()
        {
            LunaLog.Normal("Performing a GarbageCollection...");
            GC.Collect();
        }
    }
}