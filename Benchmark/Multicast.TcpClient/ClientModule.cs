using Dignus.Sockets;
using Dignus.Sockets.Interfaces;

namespace Multicast.TcpClient
{
    internal class ClientModule : ClientBase
    {
        private bool _isConnect = false;
        private ISession _session;

        public ClientModule(SessionConfiguration sessionConfiguration) : base(sessionConfiguration)
        {
        }
        public void SendMessage(byte[] message, int count)
        {
            for (int i = 0; i < count; i++)
            {
                _session.SendAsync(message);
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
