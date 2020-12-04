using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerScreenshotSettings
    {
        public int MinScreenshotIntervalMs => ScreenshotSettings.SettingsStore.MinScreenshotIntervalMs;
        public int MaxScreenshotsPerUser => ScreenshotSettings.SettingsStore.MaxScreenshotsPerUser;
        public int MaxScreenshotsFolders => ScreenshotSettings.SettingsStore.MaxScreenshotsFolders;
    }
}
