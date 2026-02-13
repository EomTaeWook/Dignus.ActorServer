using ConsoleApp.Messages;
using ConsoleApp.Networks;
using ConsoleApp.Networks.PacketFramer;
using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp
{
    internal class EchoServer : ActorTlsServerBase<PlayerActor>
    {
        public EchoServer(X509Certificate2 serverCertificate) : base(serverCertificate, new MessageSerializer(), new MyPacketFramer())
        {
            
        }
        protected override PlayerActor CreateSessionActor(IActorRef transportActorRef)
        {
            return new PlayerActor(transportActorRef);
        }

        protected override void OnAccepted(IActorRef connectedActorRef)
        {
            
        }

        protected override void OnDisconnected(IActorRef connectedActorRef)
        {
            
        }
     }
}
