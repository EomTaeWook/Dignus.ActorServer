using Dignus.Actor.Core;
using Dignus.Actor.Core.DeadLetter;
using Dignus.Actor.Network;
using Dignus.Log;
using TcpActorServer.Messages;
using TcpActorServer.Middleware;
using TcpActorServer.Networks;
using TcpActorServer.Networks.PacketFramer;

namespace TcpActorServer
{
    internal class TcpServer : TcpServerBase<EchoActor>
    {
        public TcpServer() : base(new MessageSerializer(), new MyPacketFramer())
        {
            ActorProtocolPipeline<ClientContext>.Register<Protocol>((method, context) =>
            {
            });


        }
        protected override EchoActor CreateSessionActor()
        {
            return new EchoActor();
        }
        protected override int GetRequestedDispatcherIndex()
        {
            return 1;
        }

        protected override void OnAccepted(INetworkSessionRef connectedActorRef)
        {
            LogHelper.Info($"OnAccepted : {connectedActorRef}");
        }

        protected override void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage)
        {
            LogHelper.Error($"OnDeadLetterMessage : {deadLetterMessage}");
        }

        protected override void OnDisconnected(INetworkSessionRef connectedActorRef)
        {
            LogHelper.Info($"OnDisconnected : {connectedActorRef}");
        }
     }
}
