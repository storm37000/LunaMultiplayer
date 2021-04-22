﻿using LmpCommon;
using Server.Context;
using Server.Events;
using Server.Log;
using Server.Settings.Structures;
using Server.Web.Handlers;
using Server.Web.Structures;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Handlers.Compression;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace Server.Web
{
    /// <summary>
    /// This class controls the information shown in JSON format when you go to http://yourip:8900
    /// </summary>
    public static class WebServer
    {
        private static HttpServer Server;
        public static ServerInformation ServerInformation { get; private set; }

        static WebServer() => ExitEvent.ServerClosing += StopWebServer;

        /// <summary>
        /// Starts the web server
        /// </summary>
        private static void StartWebServer()
        {
            try
            {
                if (!LunaNetUtils.IsTcpPortInUse(WebsiteSettings.SettingsStore.Port))
                {
                    Server = new HttpServer(new HttpRequestProvider());
                    ServerInformation = new ServerInformation();
                    Server.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Any, WebsiteSettings.SettingsStore.Port)));
                    Server.Use(new ExceptionHandler());
                    Server.Use(new CompressionHandler(DeflateCompressor.Default, GZipCompressor.Default));
                    Server.Use(new FileHandler());
                    Server.Use(new HttpRouter().With(string.Empty, new RestHandler<ServerInformation>(new ServerInformationRestController(), JsonResponseProvider.Default)));
                    Server.Start();
                }
                else
                {
                    LunaLog.Error("Could not start web server. Port is already in use.");
                    LunaLog.Info("You can change the web server settings inside 'Config/WebsiteSettings.xml'");
                }
            }
            catch (Exception e)
            {
                LunaLog.Error($"Could not start web server. Details: {e}");
            }
        }

        /// <summary>
        /// Starts the web server
        /// </summary>
        private static void StopWebServer()
        {
            if (WebsiteSettings.SettingsStore.EnableWebsite)
            {
                try
                {
                    Server.Dispose();
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Could not stop web server." + Environment.NewLine + $"Details: {e}");
                }
            }
        }

        /// <summary>
        /// Refresh the server information
        /// </summary>
        public static async void RefreshWebServerInformation()
        {
            if (WebsiteSettings.SettingsStore.EnableWebsite)
            {
                if (ServerInformation == null) { StartWebServer(); }
                while (ServerContext.ServerRunning)
                {
                    ServerInformation.Refresh();
                    await Task.Delay((int)TimeSpan.FromMilliseconds(WebsiteSettings.SettingsStore.RefreshIntervalMs).TotalMilliseconds);
                }
            }
        }
    }
}
