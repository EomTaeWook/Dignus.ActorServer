using Dignus.Sockets.Interfaces;
using System.Text;

namespace EchoClient.Packets
{
    internal class Packet : IPacket
    {
        public int Protocol { get; }
        public byte[] Body { get; }

        public Packet(int protocol, byte[] body)
        {
            Protocol = protocol;
            Body = body;
        }
        public Packet(int protocol, string value)
        {
            Protocol = protocol;
            Body = Encoding.UTF8.GetBytes(value);
        }
        public int GetLength()
        {
            return Body.Length + 4;
        }
    }
}
