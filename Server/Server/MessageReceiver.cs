using Lidgren.Network;
using LmpCommon;
using LmpCommon.Enums;
using LmpCommon.Message.Interface;
using LmpCommon.Time;
using Server.Client;
using Server.Context;
using Server.Message;
using Server.Plugin;

namespace Server.Server
{
    public class MessageReceiver
    {
        public void ReceiveCallback(ClientStructure client, NetIncomingMessage msg)
        {
            if (client == null || msg.LengthBytes <= 1) return;

            if (client.ConnectionStatus == ConnectionStatus.Connected)
                client.LastReceiveTime = ServerContext.ServerClock.ElapsedMilliseconds;

            if (!(ServerContext.ClientMessageFactory.Deserialize(msg, LunaNetworkTime.UtcNow.Ticks) is IClientMessageBase message)) return;

            LmpPluginHandler.FireOnMessageReceived(client, message);
            //A plugin has handled this message and requested suppression of the default behavior
            if (message.Handled) return;

            if (message.VersionMismatch)
            {
                MessageQueuer.SendConnectionEnd(client, $"Version mismatch: Your version ({message.Data.MajorVersion}.{message.Data.MinorVersion}.{message.Data.BuildVersion}) " +
                                                        $"does not match the server version: {LmpVersioning.CurrentVersion}.");
                return;
            }

            //Clients can only send HANDSHAKE until they are Authenticated.
            if (!client.Authenticated && message.MessageType != ClientMessageType.Handshake)
            {
                MessageQueuer.SendConnectionEnd(client, $"You must authenticate before sending a {message.MessageType} message");
                return;
            }

            switch (message.MessageType)
            {
                case ClientMessageType.Admin:
                    AdminMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Handshake:
                    HandshakeMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Chat:
                    ChatMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.PlayerStatus:
                    PlayerStatusMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.PlayerColor:
                    PlayerColorMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Scenario:
                    ScenarioDataMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Kerbal:
                    KerbalMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Settings:
                    SettingsMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Vessel:
                    VesselMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.CraftLibrary:
                    CraftLibraryMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Flag:
                    FlagSyncMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Motd:
                    MotdMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Warp:
                    WarpControlMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Lock:
                    LockSystemMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Mod:
                    ModDataMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Groups:
                    GroupMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Facility:
                    FacilityMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.Screenshot:
                    ScreenshotMsgReader.HandleMessage(client, message);
                    break;
                case ClientMessageType.ShareProgress:
                    ShareProgressMsgReader.HandleMessage(client, message);
                    break;
                default:
                    break;
            }
        }
    }
}
