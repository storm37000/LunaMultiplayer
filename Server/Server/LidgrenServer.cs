﻿using Lidgren.Network;
using LmpCommon;
using LmpCommon.Enums;
using LmpCommon.Message.Interface;
using LmpCommon.Time;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using Server.Utilities;
using System;
using System.Net;

namespace Server.Server
{
    public class LidgrenServer
    {
        public static NetServer Server { get; private set; }
        public static MessageReceiver ClientMessageReceiver { get; set; } = new MessageReceiver();

        public static void SetupLidgrenServer()
        {
            //ServerContext.Config.LocalAddress = IPAddress.IPv6Any;
            ServerContext.Config.Port = ConnectionSettings.SettingsStore.Port;
            ServerContext.Config.AutoExpandMTU = ConnectionSettings.SettingsStore.AutoExpandMtu;
            ServerContext.Config.MaximumTransmissionUnit = ConnectionSettings.SettingsStore.MaximumTransmissionUnit;
            ServerContext.Config.MaximumConnections = GeneralSettings.SettingsStore.MaxPlayers;
            ServerContext.Config.PingInterval = (float)TimeSpan.FromMilliseconds(ConnectionSettings.SettingsStore.HearbeatMsInterval).TotalSeconds;
            ServerContext.Config.ConnectionTimeout = (float)TimeSpan.FromMilliseconds(ConnectionSettings.SettingsStore.ConnectionMsTimeout).TotalSeconds;

            if (LunaNetUtils.IsUdpPortInUse(ServerContext.Config.Port))
            {
                throw new HandledException($"Port {ServerContext.Config.Port} is already in use");
            }

            ServerContext.Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            ServerContext.Config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);

            if (LogSettings.SettingsStore.LogLevel >= LogLevels.NetworkDebug)
            {
                ServerContext.Config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            }
            if (LogSettings.SettingsStore.LogLevel >= LogLevels.VerboseNetworkDebug)
            {
                ServerContext.Config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            }
            if (DebugSettings.SettingsStore?.SimulatedLossChance < 100 && DebugSettings.SettingsStore?.SimulatedLossChance > 0)
            {
                //ServerContext.Config.SimulatedLoss = DebugSettings.SettingsStore.SimulatedLossChance / 100f;
            }
            if (DebugSettings.SettingsStore?.SimulatedDuplicatesChance < 100 && DebugSettings.SettingsStore?.SimulatedLossChance > 0)
            {
                //ServerContext.Config.SimulatedDuplicatesChance = DebugSettings.SettingsStore.SimulatedDuplicatesChance / 100f;
            }
            //ServerContext.Config.SimulatedRandomLatency = (float)TimeSpan.FromMilliseconds((double)DebugSettings.SettingsStore?.MaxSimulatedRandomLatencyMs).TotalSeconds;
            //ServerContext.Config.SimulatedMinimumLatency = (float)TimeSpan.FromMilliseconds((double)DebugSettings.SettingsStore?.MinSimulatedLatencyMs).TotalSeconds;

            Server = new NetServer(ServerContext.Config);
            Server.Start();
        }

        public static async void StartReceivingMessages()
        {
            try
            {
                while (ServerContext.ServerRunning)
                {
                    var msg = Server.WaitMessage(ServerContext.PlayerCount > 0 ? IntervalSettings.SettingsStore.SendReceiveThreadTickMs : int.MaxValue);
                    if (msg != null)
                    {
                        var client = TryGetClient(msg);
                        switch (msg.MessageType)
                        {
                            case NetIncomingMessageType.ConnectionApproval:
                                if (ServerContext.UsePassword)
                                {
                                    var password = msg.ReadString();
                                    if (password != GeneralSettings.SettingsStore.Password)
                                    {
                                        msg.SenderConnection.Deny("Invalid password");
                                        break;
                                    }
                                }
                                msg.SenderConnection.Approve();
                                break;
                            case NetIncomingMessageType.Data:
                                ClientMessageReceiver.ReceiveCallback(client, msg);
                                client.BytesReceived += (uint)msg.LengthBytes;
                                break;
                            case NetIncomingMessageType.WarningMessage:
                                LunaLog.Warning(msg.ReadString());
                                break;
                            case NetIncomingMessageType.DebugMessage:
                                LunaLog.NetworkDebug(msg.ReadString());
                                break;
                            case NetIncomingMessageType.ConnectionLatencyUpdated:
                            case NetIncomingMessageType.VerboseDebugMessage:
                                LunaLog.NetworkVerboseDebug(msg.ReadString());
                                break;
                            case NetIncomingMessageType.ErrorMessage:
                            case NetIncomingMessageType.Error:
                                LunaLog.Error(msg.ReadString());
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                switch ((NetConnectionStatus)msg.ReadByte())
                                {
                                    case NetConnectionStatus.Connected:
                                        var endpoint = msg.SenderConnection.RemoteEndPoint;
                                        LunaLog.Normal($"New client Connection from {endpoint.Address}:{endpoint.Port}");
                                        ClientConnectionHandler.ConnectClient(msg.SenderConnection);
                                        break;
                                    case NetConnectionStatus.Disconnected:
                                        var reason = msg.ReadString();
                                        if (client != null)
                                            ClientConnectionHandler.DisconnectClient(client, reason);
                                        break;
                                }
                                break;
                            default:
                                var details = msg.PeekString();
                                LunaLog.Warning($"Unhandled Lidgren Message: {msg.MessageType.ToString().ToUpper()} -- {details}");
                                break;
                        }
                    }
//                    else
//                    {
//                        await Task.Delay(IntervalSettings.SettingsStore.SendReceiveThreadTickMs);
//                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.Fatal($"ERROR in thread receive! Details: {e}");
            }
        }

        private static ClientStructure TryGetClient(NetIncomingMessage msg)
        {
            if (msg.SenderConnection != null)
            {
                ServerContext.Clients.TryGetValue(msg.SenderConnection.RemoteEndPoint, out var client);
                return client;
            }
            return null;
        }

        public static void SendMessageToClient(ClientStructure client, IServerMessageBase message)
        {
            var outmsg = Server.CreateMessage(message.GetMessageSize());

            message.Data.SentTime = LunaNetworkTime.UtcNow.Ticks;
            message.Serialize(outmsg);

            client.LastSendTime = ServerContext.ServerClock.ElapsedMilliseconds;
            client.BytesSent += (uint)outmsg.LengthBytes;

            Server.SendMessage(outmsg, client.Connection, message.NetDeliveryMethod, message.Channel);

            //Force send of packets
            Server.FlushSendQueue();
        }
    }
}
