using Dignus.Log;
using Dignus.Sockets.Attributes;
using Dignus.Sockets.Interfaces;
using EchoClient.Packets;
using EchoClient.Protocol;
using System.Text.Json;

namespace EchoClient.Handler
{
    internal class EchoHandler : IProtocolHandler<string>, ISessionComponent
    {
        private ISession _session;
        public double MaxRttMs { get; private set; } = -1;
        public double MinRttMs { get; private set; } = 999999;

        private long _receivedCount;
        private DateTime _lastSendTime;

        public T DeserializeBody<T>(string body)
        {
            return JsonSerializer.Deserialize<T>(body);
        }

        [ProtocolName("EchoMessageResponse")]
        public void Process(EchoMessage echo)
        {
            SendEcho(echo.Message);

            var receiveTime = DateTime.UtcNow;
            var rtt = (receiveTime - _lastSendTime).TotalMilliseconds;

            if (rtt > MaxRttMs)
            {
                MaxRttMs = rtt;
            }
            if (rtt < MinRttMs)
            {
                MinRttMs = rtt;
            }
            Interlocked.Increment(ref _receivedCount);
        }

        public void OtherMessageResponse(OtherMessageResponse otherMeesage)
        {
            LogHelper.Info($"process other message");
        }

        public void SendEcho(string message)
        {
            var session = _session;
            if (session == null)
            {
                return;
            }

            var body = JsonSerializer.Serialize(new EchoMessage()
            {
                Message = message
            });

            _lastSendTime = DateTime.UtcNow;
            session.SendAsync(new Packet((int)CSProtocol.EchoMessage, body));
        }
        public void SetSession(ISession session)
        {
            _session = session;
        }

        public void Dispose()
        {
            Monitor.Instance.AddReceivedCount(_receivedCount);
            Monitor.Instance.SetMaxRttMs(MaxRttMs);
            Monitor.Instance.SetMinRttMs(MinRttMs);
            Monitor.Instance.AddClientCount(1);
            _session = null;
        }
    }
}
