using Lidgren.Network;
using LmpCommon;
using LmpCommon.Enums;
using Server.Context;
using System.Net;

namespace Server.Client
{
    public class ClientStructure
    {
        public IPEndPoint Endpoint => Connection.RemoteEndPoint;

        public string UniqueIdentifier { get; set; }

        public bool Authenticated { get; set; }

        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public NetConnection Connection { get; }

        public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Connected;
        public bool DisconnectClient { get; set; }
        public long LastReceiveTime { get; set; } = ServerContext.ServerClock.ElapsedMilliseconds;
        public long LastSendTime { get; set; } = 0;
        public float[] PlayerColor { get; set; } = new float[3];
        public string PlayerName { get; set; } = "Unknown";
        public PlayerStatus PlayerStatus { get; set; } = new PlayerStatus();
        public int Subspace { get; set; } = int.MinValue; //Leave it as min value. When client connect we force them client side to go to latest subspace
        public float SubspaceRate { get; set; } = 1f;

        public ClientStructure(NetConnection playerConnection)
        {
            Connection = playerConnection;
        }

        public override bool Equals(object obj)
        {
            var clientToCompare = obj as ClientStructure;
            return Endpoint.Equals(clientToCompare?.Endpoint);
        }

        public override int GetHashCode()
        {
            return Endpoint?.GetHashCode() ?? 0;
        }
    }
}
