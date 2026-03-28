using Dignus.Actor.Abstractions;
using Dignus.Actor.Network.Codec;
using Dignus.Actor.Network.Messages;
using Dignus.Sockets.Interfaces;
using TlsActorServer.Messages;

namespace TlsActorServer.Networks.Codecs
{
    internal class MessageSerializer : IActorMessageSerializer
    {
        public IActorMessage Deserialize(ArraySegment<byte> bytes)
        {
            return new BinaryMessage(bytes);
        }

        public ArraySegment<byte> MakeSendBuffer(INetworkActorMessage packet)
        {
            return new ArraySegment<byte>();
        }

        public ArraySegment<byte> MakeSendBuffer(IPacket packet)
        {
            return new ArraySegment<byte>();
        }
    }
}
