using Dignus.Actor.Abstractions;
using Dignus.Actor.Network;
using Dignus.Actor.Network.Codec;
using Dignus.Collections;
using Dignus.Framework;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using System.Text.Json;
using TcpActorServer.Messages;

namespace TcpActorServer.Networks.Codecs
{
    internal class MyPacketFramer : IActorMessageDecoder
    {
        public MyPacketFramer()
        {
        }
        public IActorMessage Deserialize(ReadOnlySpan<byte> packet)
        {
            //var protocol = packet[..4];

            //if (Singleton<ProtocolBodyTypeMapper>.Instance.ValidateProtocol(1))
            //{
            //    return null;
            //}

            //var bodyType = Singleton<ProtocolBodyTypeMapper>.Instance.GetBodyType(0);
            //var body = JsonSerializer.Deserialize(packet.Slice(4, packet.Length), bodyType);
            //

            return new BinaryMessage(packet.ToArray());
        }

        public bool TryFrame(ISession session, ArrayQueue<byte> buffer, out ArraySegment<byte> packet, out int consumedBytes)
        {
            consumedBytes = buffer.Count;

            return buffer.TrySlice(out packet, consumedBytes);
        }
    }
}
