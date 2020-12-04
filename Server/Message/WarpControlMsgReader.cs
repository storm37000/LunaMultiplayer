using System;
using LmpCommon.Message.Data.Warp;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using Server.Client;
using Server.System;

namespace Server.Message
{
    public class WarpControlMsgReader
    {
        public static void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = message.Data as WarpBaseMsgData;
            switch (messageData?.WarpMessageType)
            {
                case WarpMessageType.NewSubspace:
                    WarpSystemReceiver.HandleNewSubspace(client, (WarpNewSubspaceMsgData)messageData);
                    break;
                case WarpMessageType.ChangeSubspace:
                    WarpSystemReceiver.HandleChangeSubspace(client, (WarpChangeSubspaceMsgData)messageData);
                    break;
                case WarpMessageType.SubspacesRequest:
                    WarpSystemReceiver.HandleSubspaceRequest(client);
                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                default:
                    throw new NotImplementedException("Warp Type not implemented");
            }
        }
    }
}
