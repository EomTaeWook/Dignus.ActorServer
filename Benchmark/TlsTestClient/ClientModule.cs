using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Tls;

namespace TlsTestClient
{
    internal class ClientModule : TlsClientBase
    {
        private bool _isConnect = false;
        private ISession _session;

        public ClientModule(SessionConfiguration sessionConfiguration, TlsClientOptions tlsClientOptions) : base(sessionConfiguration, tlsClientOptions)
        {
        }
        public void SendMessage(byte[] message, int count)
        {
            for (int i = 0; i < count; i++)
            {
                //Send(message);
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
