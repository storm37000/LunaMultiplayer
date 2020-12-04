using LmpCommon.Message.Data;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Plugin;
using Server.Server;

namespace Server.Message
{
    public class ModDataMsgReader
    {
        public static void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (ModMsgData)message.Data;
            if (data.Relay)
                MessageQueuer.RelayMessage<ModSrvMsg>(client, data);
            LmpModInterface.OnModMessageReceived(client, data.ModName, data.Data, data.NumBytes);
        }
    }
}
