using ConsoleApp.Messages;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Protocol;
using Dignus.Collections;
using Dignus.Sockets.Interfaces;
using System.Text;

namespace ConsoleApp.Networks.PacketFramer
{
    internal class MyPacketFramer : IMessageDecoder
    {
        public IActorMessage Deserialize(ReadOnlySpan<byte> packet)
        {
            var json = Encoding.UTF8.GetString(packet);

            return new JsonMessage()
            {
                Body = json
            };
        }

        public bool TryFrame(ISession session, ArrayQueue<byte> buffer, out ArraySegment<byte> packet, out int consumedBytes)
        {
            packet = null;
            consumedBytes = 0;
            return true;
        }
    }
}
