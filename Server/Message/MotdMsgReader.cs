﻿using System;
using LmpCommon.Message.Data.Motd;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Server;
using Server.Settings.Structures;

namespace Server.Message
{
    public class MotdMsgReader
    {
        public static void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            //We don't use this message anymore so we can recycle it
            message.Recycle();

            var newMotd = GeneralSettings.SettingsStore.ServerMotd;

            if (newMotd.Length > 255)
                newMotd = newMotd.Substring(0, 255); //We don't wanna send a huuuge message!

            newMotd = newMotd
                .Replace("%Name%", client.PlayerName)
                .Replace(@"\n", Environment.NewLine)
                .Replace("%ServerName%", GeneralSettings.SettingsStore.ServerName)
                .Replace("%PlayerCount%", ServerContext.Clients.Count.ToString());

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<MotdReplyMsgData>();
            msgData.MessageOfTheDay = newMotd;

            MessageQueuer.SendToClient<MotdSrvMsg>(client, msgData);
        }
    }
}
