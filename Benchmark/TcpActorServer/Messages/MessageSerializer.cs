using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Messages;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets.Interfaces;
using System.Buffers;

namespace TcpActorServer.Messages
{
    internal class MessageSerializer : IActorMessageSerializer
    {
        public IActorMessage Deserialize(ArraySegment<byte> bytes)
        {
            return new BinaryMessage(bytes);
        }

        public ArraySegment<byte> MakeSendBuffer(IPacket packet)
        {
            return new ArraySegment<byte>();
        }

        public ArraySegment<byte> MakeSendBuffer(INetworkActorMessage packet)
        {
            if(packet is BinaryMessage binaryMessage)
            {
                return binaryMessage.Data;
            }
            return null;
        }
    }
}
