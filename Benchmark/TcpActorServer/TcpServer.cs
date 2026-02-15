using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network;
using Dignus.Log;
using TcpActorServer.Messages;
using TcpActorServer.Networks;
using TcpActorServer.Networks.PacketFramer;

namespace TcpActorServer
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

        protected override void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage)
        {
            LogHelper.Error($"OnDeadLetterMessage : {deadLetterMessage}");
        }

        protected override void OnDisconnected(IActorRef connectedActorRef)
        {
            LogHelper.Info($"OnDisconnected : {connectedActorRef}");
        }
     }
}
