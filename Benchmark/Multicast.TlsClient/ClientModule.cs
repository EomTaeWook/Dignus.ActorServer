using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Tls;

namespace Multicast.TcpClient
{
    internal class ClientModule : TlsClientBase
    {
        private bool _isConnect = false;
        private ISession _session;

        public ClientModule(SessionConfiguration sessionConfiguration, TlsClientOptions tlsClientOptions) : base(sessionConfiguration, tlsClientOptions)
        {
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
