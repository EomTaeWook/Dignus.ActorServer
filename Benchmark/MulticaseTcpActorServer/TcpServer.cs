using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network;
using Dignus.Log;
using Multicast.TcpActorServer.Networks;
using Multicast.TcpActorServer.Networks.PacketFramer;

namespace Multicast.TcpActorServer
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
            //LogHelper.Info($"OnAccepted : {connectedActorRef}");
        }

        protected override void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage)
        {
            LogHelper.Error($"OnDeadLetterMessage : {deadLetterMessage}");
        }

        protected override void OnDisconnected(IActorRef connectedActorRef)
        {
            //LogHelper.Info($"OnDisconnected : {connectedActorRef}");
        }
     }
}
