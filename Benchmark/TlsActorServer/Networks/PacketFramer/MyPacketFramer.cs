using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Protocol;
using Dignus.Collections;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using TlsActorServer.Messages;

namespace TlsActorServer.Networks.PacketFramer
{
    internal class MyPacketFramer : IMessageDecoder
    {
        public IActorMessage Deserialize(ReadOnlySpan<byte> packet)
        {
            return new BinaryMessage(packet.ToArray());
        }

        public bool TryFrame(ISession session, ArrayQueue<byte> buffer, out ArraySegment<byte> packet, out int consumedBytes)
        {
            consumedBytes = buffer.Count;

            return buffer.TrySlice(out packet, consumedBytes);
        }
    }
}
