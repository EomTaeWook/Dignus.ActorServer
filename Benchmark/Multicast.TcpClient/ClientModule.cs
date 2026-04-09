using Dignus.Sockets;
using Dignus.Sockets.Interfaces;

namespace Multicast.TcpClient
{
    internal class ClientModule(SessionConfiguration sessionConfiguration) : TcpClientBase(sessionConfiguration)
    {
        private bool _isConnect = false;
        private ISession? _session;

        public void SendMessage(byte[] message, int count)
        {
            for (int i = 0; i < count; i++)
            {
                _session?.SendAsync(message);
            }
        }

        protected override void OnConnected(ISession session)
        {
            _session = session;
            _isConnect = true;
        }

        protected override void OnDisconnected(ISession session)
        {
            _isConnect = false;
        }
        public bool IsConnect => _isConnect;
    }
}
