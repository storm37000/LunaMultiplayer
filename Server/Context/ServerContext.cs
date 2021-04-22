using Lidgren.Network;
using LmpCommon.Message;
using Server.Client;
using Server.Server;
using Server.Settings.Structures;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using Server.Log;

namespace Server.Context
{
    public static class ServerContext
    {
        public static int PlayerCount => ClientRetriever.GetActiveClientCount();
        public static readonly ConcurrentDictionary<IPEndPoint, ClientStructure> Clients = new ConcurrentDictionary<IPEndPoint, ClientStructure>();

        public static volatile bool ServerRunning = false;
        public static volatile bool ConfigsLoaded = false;
        public static volatile byte Day;

        public static string Players => ClientRetriever.GetActivePlayerNames();
        public static bool UsePassword => !string.IsNullOrEmpty(GeneralSettings.SettingsStore.Password);

        public static Stopwatch ServerClock = new Stopwatch();
        public static string ModFilePath = Path.Combine(MainServer.startdir, "LMPModControl.xml");
        public static string UniverseDirectory = Path.Combine(MainServer.startdir, "Universe");
        public static string ConfigDirectory = Path.Combine(MainServer.startdir, "Config");

        // Configuration object
        public static NetPeerConfiguration Config { get; } = new NetPeerConfiguration("LMP")
        {
            SendBufferSize = 1500000, //500kb
            ReceiveBufferSize = 1500000, //500kb
            DefaultOutgoingMessageCapacity = 500000, //500kb
            SuppressUnreliableUnorderedAcks = true,
            AutoFlushSendQueue = true,
            UseMessageRecycling = true,
            DualStack = true,
        };

        public static MasterServerMessageFactory MasterServerMessageFactory { get; } = new MasterServerMessageFactory();
        public static ServerMessageFactory ServerMessageFactory { get; } = new ServerMessageFactory();
        public static ClientMessageFactory ClientMessageFactory { get; } = new ClientMessageFactory();

        public static void Shutdown(string reason)
        {
            LunaLog.Debug($"Shutting down with {PlayerCount} Players, " + $"{Clients.Count} connected Clients");
            MessageQueuer.SendConnectionEndToAll(reason);
            ConfigsLoaded = false;
            ServerRunning = false;
            LidgrenServer.Server.Shutdown("So long and thanks for all the fish");
        }
    }
}
