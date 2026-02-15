using ConsoleApp.Messages;
using ConsoleApp.Networks;
using ConsoleApp.Networks.PacketFramer;
using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network;
using Dignus.Log;

namespace ConsoleApp
{
    internal class TcpServer : TcpServerBase<EchoActor>
    {
        public TcpServer() : base(new MessageSerializer(), new MyPacketFramer())
        {
            
        }
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
     }
}
