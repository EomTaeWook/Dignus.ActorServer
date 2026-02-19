using Dignus.Collections;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Processing;

namespace Multicast.TcpClient.Serializer
{
    internal class EchoPacketProcessor() : PacketProcessor, IPacketSerializer, ISessionComponent
    {
        private long _totalBytes = 0;

        private ISession _session;

        public void Dispose()
        {
            Monitor.Instance.AddClientCount(1);
            Monitor.Instance.AddTotalBytes(_totalBytes);
        }

        public ArraySegment<byte> MakeSendBuffer(IPacket packet)
        {
            return new ArraySegment<byte>();
        }

        public void SetSession(ISession session)
        {
            _session = session;
        }
        protected override bool TakeReceivedPacket(ISession session, ArrayQueue<byte> buffer, out ArraySegment<byte> packet, out int consumedBytes)
        {
            consumedBytes = 0;
            if (buffer.TrySlice(out packet, Consts.Message.Length) == false)
            {
                return false;
            }
            consumedBytes = Consts.Message.Length;
            return true;
        }

        protected override Task ProcessPacketAsync(ISession session, ArraySegment<byte> packet)
        {
            _totalBytes += Consts.Message.Length;
            return Task.CompletedTask;
        }
    }
}
