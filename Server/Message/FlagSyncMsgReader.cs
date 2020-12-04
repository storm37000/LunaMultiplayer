using LmpCommon.Message.Data.Flag;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using Server.Client;
using Server.System;

namespace Server.Message
{
    public class FlagSyncMsgReader
    {
        public static void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (FlagBaseMsgData)message.Data;

            switch (data.FlagMessageType)
            {
                case FlagMessageType.ListRequest:
                    FlagSystem.HandleFlagListRequestMessage(client);
                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                case FlagMessageType.FlagData:
                    FlagSystem.HandleFlagDataMessage(client, (FlagDataMsgData)data);
                    break;
            }
        }
    }
}
