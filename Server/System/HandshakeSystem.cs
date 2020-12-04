using LmpCommon.Enums;
using LmpCommon.Message.Data.Handshake;
using LmpCommon.Message.Data.PlayerConnection;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Plugin;
using Server.Server;
using Server.Command.Command;
using Server.Settings.Structures;
using System.Linq;
using System.Text.RegularExpressions;

namespace Server.System
{
    public class HandshakeSystem
    {
        private static string Reason { get; set; }

        private static bool CheckUsernameLength(ClientStructure client, string username)
        {
            if (username.Length > GeneralSettings.SettingsStore.MaxUsernameLength)
            {
                Reason = $"Username too long. Max chars: {GeneralSettings.SettingsStore.MaxUsernameLength}";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                return false;
            }

            if (username.Length <= 0)
            {
                Reason = "Username too short. Min chars: 1";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                return false;
            }

            return true;
        }

        private static bool CheckServerFull(ClientStructure client)
        {
            if (ClientRetriever.GetActiveClientCount() >= GeneralSettings.SettingsStore.MaxPlayers)
            {
                Reason = "Server full";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.ServerFull, Reason);
                return false;
            }
            return true;
        }

        private static bool CheckPlayerIsBanned(ClientStructure client, string uniqueId)
        {
            if (BanPlayerCommand.GetBannedPlayers().Contains(uniqueId))
            {
                Reason = "Banned";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.PlayerBanned, Reason);
                return false;
            }
            return true;
        }

        private static bool CheckUsernameIsReserved(ClientStructure client, string playerName)
        {
            if (playerName == "Initial" || playerName == GeneralSettings.SettingsStore.ConsoleIdentifier)
            {
                Reason = "Using reserved name";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                return false;
            }
            return true;
        }

        private static bool CheckPlayerIsAlreadyConnected(ClientStructure client, string playerName)
        {
            var existingClient = ClientRetriever.GetClientByName(playerName);
            if (existingClient != null)
            {
                Reason = "Username already taken";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                return false;
            }
            return true;
        }

        private static bool CheckUsernameCharacters(ClientStructure client, string playerName)
        {
            var regex = new Regex(@"^[-_a-zA-Z0-9]+$"); // Regex to only allow alphanumeric, dashes and underscore
            if (!regex.IsMatch(playerName))
            {
                Reason = "Invalid username characters (only A-Z, a-z, numbers, - and _)";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                return false;
            }
            return true;
        }

        public static void HandleHandshakeRequest(ClientStructure client, HandshakeRequestMsgData data)
        {
            var valid = CheckServerFull(client);
            valid &= valid && CheckUsernameLength(client, data.PlayerName);
            valid &= valid && CheckUsernameCharacters(client, data.PlayerName);
            valid &= valid && CheckPlayerIsAlreadyConnected(client, data.PlayerName);
            valid &= valid && CheckUsernameIsReserved(client, data.PlayerName);
            valid &= valid && CheckPlayerIsBanned(client, data.UniqueIdentifier);

            if (!valid)
            {
                LunaLog.Normal($"Client {data.PlayerName} ({data.UniqueIdentifier}) failed to handshake: {Reason}. Disconnecting");
                client.DisconnectClient = true;
                ClientConnectionHandler.DisconnectClient(client, Reason);
            }
            else
            {
                client.PlayerName = data.PlayerName;
                client.UniqueIdentifier = data.UniqueIdentifier;
                client.Authenticated = true;

                LmpPluginHandler.FireOnClientAuthenticated(client);

                LunaLog.Normal($"Client {data.PlayerName} ({data.UniqueIdentifier}) handshake successfully, Version: {data.MajorVersion}.{data.MinorVersion}.{data.BuildVersion}");

                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.HandshookSuccessfully, "success");

                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<PlayerConnectionJoinMsgData>();
                msgData.PlayerName = client.PlayerName;
                MessageQueuer.RelayMessage<PlayerConnectionSrvMsg>(client, msgData);

                LunaLog.Debug($"Online Players: {ServerContext.PlayerCount}, connected: {ClientRetriever.GetClients().Length}");
            }
        }
    }
}
