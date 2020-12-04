using Microsoft.VisualStudio.Threading;
using Open.Nat;
using Server.Context;
using Server.Events;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Upnp
{
    public static class LmpPortMapper
    {
        private static readonly int LifetimeInSeconds = (int)TimeSpan.FromMinutes(5).TotalSeconds;
        private static readonly AsyncLazy<NatDevice> Device = new AsyncLazy<NatDevice>(DiscoverDevice);

        private static Mapping LmpPortMapping => new Mapping(Protocol.Udp, ConnectionSettings.SettingsStore.Port, ConnectionSettings.SettingsStore.Port,
            LifetimeInSeconds, $"LMPServer {ConnectionSettings.SettingsStore.Port}");

        private static Mapping LmpWebPortMapping => new Mapping(Protocol.Tcp, WebsiteSettings.SettingsStore.Port, WebsiteSettings.SettingsStore.Port,
            LifetimeInSeconds, $"LMPServerWeb {WebsiteSettings.SettingsStore.Port}");

        private static async Task<NatDevice> DiscoverDevice()
        {
            var nat = new NatDiscoverer();
            return await nat.DiscoverDeviceAsync(PortMapper.Upnp, new CancellationTokenSource(ConnectionSettings.SettingsStore.UpnpMsTimeout));
        }

        static LmpPortMapper() => ExitEvent.ServerClosing += () =>
        {
            CloseLmpPort().Wait();
            CloseWebPort().Wait();
        };

        /// <summary>
        /// Opens the port set in the settings using UPnP. With a lifetime of <see cref="LifetimeInSeconds"/> seconds
        /// </summary>
        [DebuggerHidden]
        public static async Task OpenLmpPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.CreatePortMapAsync(LmpPortMapping);
                    LunaLog.Debug($"UPnP opened game port: {ConnectionSettings.SettingsStore.Port} protocol: {LmpPortMapping.Protocol}");
                }
                catch (Exception e)
                {
                    LunaLog.Error($"UPnP failed to open game port, manual port forwarding may be required if players can't join! Your router likely doesnt have UPnP enabled! Disable UPnp in ConnectionSettings.xml to get rid of this message. (" + e.Message + ")");
                }
            }
        }

        /// <summary>
        /// Opens the website port set in the settings using UPnP. With a lifetime of <see cref="LifetimeInSeconds"/> seconds
        /// </summary>
        [DebuggerHidden]
        public static async Task OpenWebPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp && WebsiteSettings.SettingsStore.EnableWebsite)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.CreatePortMapAsync(LmpWebPortMapping);
                    LunaLog.Debug($"UPnP opened http port: {WebsiteSettings.SettingsStore.Port} protocol: {LmpWebPortMapping.Protocol}");
                }
                catch (Exception e)
                {
                    LunaLog.Error($"UPnP failed to open http port, manual port forwarding may be required if players can't join! Your router likely doesnt have UPnP enabled! Disable UPnp in ConnectionSettings.xml to get rid of this message. (" + e.Message + ")");
                }
            }
        }

        /// <summary>
        /// Refresh the UPnP port every 1 minute
        /// </summary>
        public static async void RefreshUpnpPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp)
            {
                while (ServerContext.ServerRunning)
                {
                    await OpenLmpPort();
                    await OpenWebPort();
                    await Task.Delay((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
                }
            }
        }

        /// <summary>
        /// Closes the opened port using UPnP
        /// </summary>
        [DebuggerHidden]
        public static async Task CloseLmpPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp && ServerContext.ServerRunning)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.DeletePortMapAsync(LmpPortMapping);
                    LunaLog.Debug($"UPnP closed game port: {ConnectionSettings.SettingsStore.Port} protocol: {LmpPortMapping.Protocol}");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Closes the opened web port using UPnP
        /// </summary>
        [DebuggerHidden]
        public static async Task CloseWebPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp && WebsiteSettings.SettingsStore.EnableWebsite && ServerContext.ServerRunning)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.DeletePortMapAsync(LmpWebPortMapping);
                    LunaLog.Debug($"UPnP closed http port: {WebsiteSettings.SettingsStore.Port} protocol: {LmpWebPortMapping.Protocol}");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Gets external IP using UPnP
        /// </summary>
        public static async Task<IPAddress> GetExternalIp()
        {
            var device = await Device.GetValueAsync();
            return await device.GetExternalIPAsync();
        }
    }
}
