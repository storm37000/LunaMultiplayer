using System;
using LmpCommon.Message.Data.Handshake;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using Server.Client;
using Server.System;

namespace Server.Message
{
    public class HandshakeMsgReader
    {
        public static void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = message.Data as HandshakeBaseMsgData;
            switch (data?.HandshakeMessageType)
            {
                case HandshakeMessageType.Request:
                    HandshakeSystem.HandleHandshakeRequest(client, (HandshakeRequestMsgData)data);
                    break;
                default:
                    throw new NotImplementedException("Handshake type not implemented");
            }

            message.Recycle();
        }
    }
}
