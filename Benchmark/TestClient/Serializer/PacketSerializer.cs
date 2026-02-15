using Dignus.Collections;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Processing;
using EchoClient.Handler;
using EchoClient.Packets;
using System.Text;

namespace EchoClient.Serializer
{
    internal class PacketSerializer(EchoHandler echoHandler) : PacketHandlerBase, IPacketSerializer
    {
        private const int SizeToInt = sizeof(int);
        private const int HeaderSize = sizeof(int) * 2;

        public ArraySegment<byte> MakeSendBuffer(IPacket packet)
        {
            if (packet is Packet sendPacket == false)
            {
                throw new InvalidCastException(nameof(packet));
            }
            var packetSize = sendPacket.GetLength();
            byte[] sendBuffer = new byte[SizeToInt + packetSize];

            Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, sendBuffer, 0, SizeToInt);
            Buffer.BlockCopy(BitConverter.GetBytes(sendPacket.Protocol), 0, sendBuffer, SizeToInt, SizeToInt);
            Buffer.BlockCopy(sendPacket.Body, 0, sendBuffer, HeaderSize, sendPacket.Body.Length);
            return sendBuffer;
        }

        public async override Task ProcessPacketAsync(ArraySegment<byte> packet)
        {
            var protocol = BitConverter.ToInt32(packet.Array, packet.Offset);
            var size = packet.Array.Length - packet.Offset - SizeToInt;
            var bodyString = Encoding.UTF8.GetString(packet.Array, packet.Offset + SizeToInt, size);

            await ProtocolHandlerMapper<EchoHandler, string>.InvokeHandlerAsync(echoHandler, protocol, bodyString);
        }

        public override bool TakeReceivedPacket(ArrayQueue<byte> buffer, out ArraySegment<byte> packet, out int consumedBytes)
        {
            packet = null;
            consumedBytes = 0;
            if (buffer.Count < SizeToInt)
            {
                return false;
            }

            var headerBytes = buffer.Peek(SizeToInt);

            consumedBytes = BitConverter.ToInt32(headerBytes);

            if (buffer.Count < consumedBytes + SizeToInt)
            {
                consumedBytes = 0;
                return false;
            }

            if (buffer.TryReadBytes(out _, SizeToInt) == false)
            {
                consumedBytes = 0;
                return false;
            }

            if (buffer.TrySlice(out packet, consumedBytes) == false)
            {
                consumedBytes = 0;
                return false;
            }
            return true;
        }
    }
}
