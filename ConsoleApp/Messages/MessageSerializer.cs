using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets.Interfaces;
using System.Text;

namespace ConsoleApp.Messages
{
    internal class MessageSerializer : IActorMessageSerializer
    {
        public IActorMessage Deserialize(ArraySegment<byte> bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);

            return new JsonMessage()
            {
                Body = json
            };
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
