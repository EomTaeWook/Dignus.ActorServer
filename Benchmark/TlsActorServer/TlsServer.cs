using Dignus.Actor.Core.DeadLetter;
using Dignus.Actor.Network;
using Dignus.Framework;
using Dignus.Log;
using Dignus.Sockets.Interfaces;
using System.Security.Cryptography.X509Certificates;
using TcpActorServer.Messages;
using TlsActorServer.Networks;
using TlsActorServer.Networks.Codecs;

namespace TlsActorServer
{
    internal class TlsServer : TlsServerBase<EchoActor>
    {
        public TlsServer(X509Certificate2 certificate2) : base(certificate2, new MessageSerializer(), new MyPacketFramer())
        {
            Singleton<ProtocolBodyTypeMapper>.Instance.AddMapping<EchoMessage>(CSProtocol.EchoMessage);
        }
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
