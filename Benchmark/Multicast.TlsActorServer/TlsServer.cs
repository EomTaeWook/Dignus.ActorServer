using Dignus.Actor.Core.DeadLetter;
using Dignus.Actor.Network;
using Dignus.Actor.Network.Options;
using Dignus.Log;
using Dignus.Sockets.Interfaces;
using Multicast.TlsActorServer.Networks;
using System.Collections.Concurrent;

namespace Multicast.TlsActorServer
{
    internal class TlsServer(TlsServerOptions tlsServerOptions) : TlsServerBase<EchoActor>(tlsServerOptions)
    {
        private ConcurrentDictionary<INetworkSession, byte> _sessions = new ConcurrentDictionary<INetworkSession, byte>();
        protected override EchoActor CreateSessionActor()
        {
            return new EchoActor();
        }

        protected override void OnAccepted(INetworkSessionRef connectedSessionRef)
        {
            _sessions.TryAdd(connectedSessionRef, 0);
            //LogHelper.Info($"OnAccepted : {connectedActorRef}");
        }

        protected override void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage)
        {
            LogHelper.Info($"OnDeadLetterMessage : {deadLetterMessage}");
        }

        protected override void OnDisconnected(INetworkSessionRef disconnectedSessionRef)
        {
            _sessions.TryRemove(disconnectedSessionRef, out _);
            LogHelper.Info($"OnDisconnected : {disconnectedSessionRef}");
        }

        protected override void OnHandshakeFailed(ISession session, Exception ex)
        {
            LogHelper.Info($"OnHandshakeFailed : {session}");
        }
        public void Broadcast(byte[] bytes)
        {
            foreach (var session in _sessions.Keys)
            {
                session.SendAsync(bytes);
            }
        }
    }
}
