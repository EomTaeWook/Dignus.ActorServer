using Dignus.Actor.Core.DeadLetter;
using Dignus.Actor.Network;
using Dignus.Framework;
using Dignus.Log;
using TcpActorServer.Messages;
using TcpActorServer.Networks;
using TcpActorServer.Networks.Codecs;

namespace TcpActorServer
{
    internal class TcpServer : TcpServerBase<EchoActor>
    {
        public TcpServer() : base(new MessageSerializer(), new MyPacketFramer())
        {
            Singleton<ProtocolBodyTypeMapper>.Instance.Register(typeof(EchoMessage).Assembly);
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
            LogHelper.Info($"OnAccepted : {connectedActorRef.GetHashCode()}");
        }

        protected override void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage)
        {
            LogHelper.Error($"OnDeadLetterMessage : {deadLetterMessage}");
        }

        protected override void OnDisconnected(INetworkSessionRef connectedActorRef)
        {
            LogHelper.Info($"OnDisconnected : {connectedActorRef.GetHashCode()}");
        }
     }
}
