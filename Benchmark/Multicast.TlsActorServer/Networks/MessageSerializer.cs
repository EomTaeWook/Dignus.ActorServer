using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Messages;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets.Interfaces;
using Multicast.TlsActorServer.Messages;
using System.Buffers;

namespace Multicast.TlsActorServer.Networks
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

        public ArraySegment<byte> MakeSendBuffer(INetworkActorMessage message)
        {
            if(message is BinaryMessage binaryMessage)
            {
                return binaryMessage.Data;
            }
            return new ArraySegment<byte>();
        }
    }
}
