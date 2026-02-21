using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network;
using Dignus.Actor.Network.Actors;
using Dignus.Actor.Network.Options;
using Dignus.Log;
using Multicast.TcpActorServer.Networks;
using Multicast.TcpActorServer.Networks.PacketFramer;

namespace Multicast.TcpActorServer
{
    internal class TcpServer(ServerOptions serverOptions) : TcpServerBase<EchoActor>(serverOptions)
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
            LogHelper.Error($"OnDeadLetterMessage : {deadLetterMessage.Reason}");
        }

        protected override void OnDisconnected(IActorRef connectedActorRef)
        {
            //LogHelper.Info($"OnDisconnected : {connectedActorRef}");
        }
     }
}
