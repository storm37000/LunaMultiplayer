using LmpCommon.Time;
using Server.Client;
using Server.Command.Command.Base;
using Server.Log;

namespace Server.Command.Command
{
    public class ConnectionStatsCommand : SimpleCommand
    {
        public override bool Execute(string commandArgs)
        {
            //Do some shit here.
            ulong bytesSentTotal = 0;
            ulong bytesReceivedTotal = 0;
            LunaLog.Normal("Connection stats:");
            LunaLog.Normal($"Nist Time Difference: {LunaNetworkTime.TimeDifference.TotalMilliseconds} ms");
            foreach (var client in ClientRetriever.GetAuthenticatedClients())
            {
                bytesSentTotal += client.BytesSent;
                bytesReceivedTotal += client.BytesReceived;
                LunaLog.Normal(
                    $"Server sent: {(float)client.BytesSent/1024} KB, received: {(float)client.BytesReceived/1024} KB from '{client.PlayerName}'");
            }
            LunaLog.Normal($"Server sent: {((float)bytesSentTotal/1024)/1024} MB, received: {((float)bytesReceivedTotal/1024)/1024} MB in total amongst above players.");

            return true;
        }
    }
}
