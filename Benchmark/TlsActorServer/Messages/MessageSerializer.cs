using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Messages;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets.Interfaces;
using System.Buffers;

namespace TlsActorServer.Messages
{
    internal class MessageSerializer : IActorMessageSerializer
    {
        public IActorMessage Deserialize(ArraySegment<byte> bytes)
        {
            return new BinaryMessage(bytes.Array);
        }

        public ArraySegment<byte> MakeSendBuffer(IActorMessage packet)
        {
            return new ArraySegment<byte>();
        }

        public ArraySegment<byte> MakeSendBuffer(IPacket packet)
        {
            return new ArraySegment<byte>();
        }
    }
}
