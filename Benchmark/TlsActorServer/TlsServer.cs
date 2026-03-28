using Dignus.Actor.Core.DeadLetter;
using Dignus.Actor.Network;
using Dignus.Log;
using Dignus.Sockets.Interfaces;
using System.Security.Cryptography.X509Certificates;
using TlsActorServer.Networks;
using TlsActorServer.Networks.Codecs;

namespace TlsActorServer
{
    internal class TlsServer(X509Certificate2 certificate2) : TlsServerBase<EchoActor>(certificate2, new MessageSerializer(), new MyPacketFramer())
    {
        protected override EchoActor CreateSessionActor()
        {
            return new EchoActor();
        }

        protected override void OnAccepted(INetworkSessionRef connectedActorRef)
        {
            LogHelper.Info($"OnAccepted : {connectedActorRef}");
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
