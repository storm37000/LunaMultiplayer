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
        private static Mapping LmpPortMapping => new Mapping(Protocol.Udp, ConnectionSettings.SettingsStore.Port, ConnectionSettings.SettingsStore.Port, LifetimeInSeconds, $"LMPServer {ConnectionSettings.SettingsStore.Port}");
        private static Mapping LmpWebPortMapping => new Mapping(Protocol.Tcp, WebsiteSettings.SettingsStore.Port, WebsiteSettings.SettingsStore.Port, LifetimeInSeconds, $"LMPServerWeb {WebsiteSettings.SettingsStore.Port}");

        private static async Task<NatDevice> DiscoverDevice()
        {
            NatDevice device;
            try
            {
                return await new NatDiscoverer().DiscoverDeviceAsync(PortMapper.Upnp, new CancellationTokenSource(ConnectionSettings.SettingsStore.UpnpMsTimeout));
            }
            catch (Exception e)
            {
                LunaLog.Warning("Failed to use UPnP, trying NAT PMP... (" + e.Message + ")");
                try
                {
                    return await new NatDiscoverer().DiscoverDeviceAsync(PortMapper.Pmp, new CancellationTokenSource(ConnectionSettings.SettingsStore.UpnpMsTimeout));
                }
                catch(Exception ex)
                {
                    LunaLog.Warning("Failed to use NAT PMP! (" + ex.Message + ")");
                    return null;
                }

            }
        }

        static LmpPortMapper() => ExitEvent.ServerClosing += () =>
        {
            CloseLmpPort().Wait();
            CloseWebPort().Wait();
        };

        /// <summary>
        /// Opens the all ports set in the settings using UPnP, PMP, or STUN(coming soon). With a lifetime of <see cref="LifetimeInSeconds"/> seconds
        /// </summary>
        public static async Task OpenPorts()
        {
            if (ConnectionSettings.SettingsStore.Upnp)
            {
                var device = await Device.GetValueAsync();
                try
                {
                    await device.CreatePortMapAsync(LmpPortMapping);
                    LunaLog.Debug($"Opened game port: {LmpPortMapping.PublicPort} protocol: {LmpPortMapping.Protocol}");
                }
                catch (Exception)
                {
                    LunaLog.Error($"Failed to open game port, manual port forwarding may be required if players can't join! Your router likely doesnt have UPnP enabled! Disable UPnp in ConnectionSettings.xml to get rid of this message.");
                }
                if (WebsiteSettings.SettingsStore.EnableWebsite)
                {
                    try
                    {
                        await device.CreatePortMapAsync(LmpWebPortMapping);
                        LunaLog.Debug($"Opened http port: {LmpWebPortMapping.PublicPort} protocol: {LmpWebPortMapping.Protocol}");
                    }
                    catch (Exception)
                    {
                        LunaLog.Error($"Failed to open http port, manual port forwarding may be required if players can't join! Your router likely doesnt have UPnP enabled! Disable UPnp in ConnectionSettings.xml to get rid of this message.");
                    }
                }
            }
        }

        /// <summary>
        /// Refresh the UPnP port every 1 minute
        /// </summary>
//        public static async void RefreshUpnpPort()
//        {
//            if (ConnectionSettings.SettingsStore.Upnp)
//            {
//                while (ServerContext.ServerRunning)
//                {
//                    await OpenLmpPort();
//                    await OpenWebPort();
//                    await Task.Delay((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
//                }
//            }
//        }

        /// <summary>
        /// Closes the opened port using UPnP
        /// </summary>
        [DebuggerHidden]
        private static async Task CloseLmpPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp && ServerContext.ServerRunning)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.DeletePortMapAsync(LmpPortMapping);
                    LunaLog.Debug($"Closed game port: {ConnectionSettings.SettingsStore.Port} protocol: {LmpPortMapping.Protocol}");
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
        private static async Task CloseWebPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp && WebsiteSettings.SettingsStore.EnableWebsite && ServerContext.ServerRunning)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.DeletePortMapAsync(LmpWebPortMapping);
                    LunaLog.Debug($"Closed http port: {WebsiteSettings.SettingsStore.Port} protocol: {LmpWebPortMapping.Protocol}");
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
//        public static async Task<IPAddress> GetExternalIp()
//        {
//            var device = await Device.GetValueAsync();
//            return await device.GetExternalIPAsync();
//        }
    }
}
