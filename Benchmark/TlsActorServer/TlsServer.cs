using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network;
using Dignus.Log;
using Dignus.Sockets.Interfaces;
using System.Security.Cryptography.X509Certificates;
using TlsActorServer.Messages;
using TlsActorServer.Networks;
using TlsActorServer.Networks.PacketFramer;

namespace TlsActorServer
{
    internal class TlsServer(X509Certificate2 certificate2) : TlsServerBase<EchoActor>(certificate2, new MessageSerializer(), new MyPacketFramer())
    {
        protected override EchoActor CreateSessionActor(IActorRef transportActorRef)
        {
            return new EchoActor(transportActorRef);
        }

        protected override void OnAccepted(IActorRef connectedActorRef)
        {
            LogHelper.Info($"OnAccepted : {connectedActorRef}");
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
