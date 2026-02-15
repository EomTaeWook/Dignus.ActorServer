using Dignus.Collections;
using Dignus.Log;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Processing;
using EchoClient.Packets;

namespace EchoClient.Serializer
{
    internal class EchoPacketHandler() : PacketProcessor, IPacketSerializer, ISessionComponent
    {
        private long _totalBytes = 0;
        private readonly double _maxRttMs = -1;
        private readonly double _minRttMs = double.MaxValue;
        private readonly DateTime _lastSendTime = DateTime.MinValue;
        private ISession _session;

        public void Dispose()
        {
            Monitor.Instance.SetMaxRttMs(_maxRttMs);
            Monitor.Instance.SetMinRttMs(_minRttMs);
            Monitor.Instance.AddClientCount(1);
            Monitor.Instance.AddTotalBytes(_totalBytes);
        }

        public ArraySegment<byte> MakeSendBuffer(IPacket packet)
        {
            if (packet is Packet sendPacket == false)
            {
                throw new InvalidCastException(nameof(packet));
            }
            return sendPacket.Body;
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
            var result = session.SendAsync(packet);
            if (result != SendResult.Success && result != SendResult.Disposed)
            {
                LogHelper.Error($"{result}");
            }
            _totalBytes += Consts.Message.Length;

            return Task.CompletedTask;
        }
    }
}
