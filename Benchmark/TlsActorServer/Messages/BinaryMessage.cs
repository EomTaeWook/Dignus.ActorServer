using Dignus.Actor.Network.Messages;

namespace TlsActorServer.Messages
{
    public class BinaryMessage(ArraySegment<byte> bytes) : INetworkActorMessage
    {
        public ArraySegment<byte> Data { get; } = bytes;
    }
}
