﻿using LmpCommon.Enums;
using LmpCommon.Message.Data.Handshake;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Server;
using Server.Settings.Structures;
using LmpCommon.Xml;

namespace Server.System
{
    public class HandshakeSystemSender
    {
        public static void SendHandshakeReply(ClientStructure client, HandshakeReply enumResponse, string reason)
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<HandshakeReplyMsgData>();
            msgData.Response = enumResponse;
            msgData.Reason = reason;

            if (enumResponse == HandshakeReply.HandshookSuccessfully)
            {
                msgData.ModControl = GeneralSettings.SettingsStore.ModControl;
                msgData.ServerStartTime = TimeContext.StartTime.Ticks;

                if (GeneralSettings.SettingsStore.ModControl)
                {
                    msgData.ModFileData = LunaXmlSerializer.SerializeToXml(ModFileSystem.ModControl);
                }
            }

            MessageQueuer.SendToClient<HandshakeSrvMsg>(client, msgData);
        }
    }
}
