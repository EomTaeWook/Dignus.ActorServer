using Dignus.Actor.Abstractions;
using Dignus.Actor.Network.Codec;
using Dignus.Actor.Network.Messages;
using Dignus.Sockets.Interfaces;
using Multicast.TlsActorServer.Messages;

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
