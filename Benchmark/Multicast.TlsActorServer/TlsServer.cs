using Dignus.Actor.Core;
using Dignus.Actor.Core.DeadLetter;
using Dignus.Actor.Network;
using Dignus.Actor.Network.Options;
using Dignus.Log;
using Dignus.Sockets.Interfaces;
using Multicast.TlsActorServer.Networks;

namespace Multicast.TlsActorServer
{
    internal class TlsServer(TlsServerOptions tlsServerOptions) : TlsServerBase<EchoActor>(tlsServerOptions)
    {
        protected override EchoActor CreateSessionActor()
        {
            return new EchoActor();
        }

        protected override void OnAccepted(INetworkSessionRef connectedSessionRef)
        {
            //LogHelper.Info($"OnAccepted : {connectedActorRef}");
        }

        protected override void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage)
        {
            LogHelper.Info($"OnDeadLetterMessage : {deadLetterMessage}");
        }

        protected override void OnDisconnected(INetworkSessionRef disconnectedSessionRef)
        {
            LogHelper.Info($"OnDisconnected : {disconnectedSessionRef}");
        }

        protected override void OnHandshakeFailed(ISession session, Exception ex)
        {
            LogHelper.Info($"OnHandshakeFailed : {session}");
        }
    }
}
