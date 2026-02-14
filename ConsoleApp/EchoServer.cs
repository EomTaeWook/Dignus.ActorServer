using ConsoleApp.Messages;
using ConsoleApp.Networks;
using ConsoleApp.Networks.PacketFramer;
using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network;
using Dignus.Log;

namespace ConsoleApp
{
    internal class EchoServer : TcpServerBase<PlayerActor>
    {
        public EchoServer() : base(new MessageSerializer(), new MyPacketFramer())
        {
            
        }
        protected override PlayerActor CreateSessionActor(IActorRef transportActorRef)
        {
            return new PlayerActor(transportActorRef);
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
