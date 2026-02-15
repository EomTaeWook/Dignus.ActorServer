using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using EchoClient.Handler;

namespace EchoClient
{
    internal class ClientModule : ClientBase
    {
        private bool _isConnect = false;
        private ISession _session;
        private EchoHandler _echoHandler;

        public ClientModule(SessionConfiguration sessionConfiguration) : base(sessionConfiguration)
        {
        }
        public void SendEcho(string message)
        {
            _echoHandler.SendEcho(message);
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
            foreach (var component in session.GetSessionComponents())
            {
                if (component is EchoHandler echoHandler)
                {
                    _echoHandler = echoHandler;
                }
            }
            _isConnect = true;
        }

        protected override void OnDisconnected(ISession session)
        {
            _isConnect = false;
        }
        public bool IsConnect => _isConnect;
    }
}
