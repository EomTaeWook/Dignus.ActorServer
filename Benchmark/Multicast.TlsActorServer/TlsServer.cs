using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
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

        protected override void OnAccepted(IActorRef connectedActorRef)
        {
            //LogHelper.Info($"OnAccepted : {connectedActorRef}");
        }

        protected override void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage)
        {
            LogHelper.Info($"OnDeadLetterMessage : {deadLetterMessage}");
        }

        protected override void OnDisconnected(IActorRef connectedActorRef)
        {
            LogHelper.Info($"OnDisconnected : {connectedActorRef}");
        }

        protected override void OnHandshakeFailed(ISession session, Exception ex)
        {
            LogHelper.Info($"OnHandshakeFailed : {session}");
        }
    }
}
