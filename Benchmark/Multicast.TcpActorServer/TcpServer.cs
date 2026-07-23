using Dignus.Actor.Core.DeadLetter;
using Dignus.Actor.Network;
using Dignus.Actor.Network.Options;
using Dignus.Log;
using Multicast.TcpActorServer.Networks;
using System.Collections.Concurrent;

namespace Multicast.TcpActorServer
{
    internal class TcpServer(ServerOptions serverOptions) : TcpServerBase<EchoActor>(serverOptions)
    {
        private ConcurrentDictionary<INetworkSession, byte> _sessions = new ConcurrentDictionary<INetworkSession, byte>();
        protected override EchoActor CreateSessionActor()
        {
            return new EchoActor();
        }

        protected override void OnAccepted(INetworkSessionRef connectedActorRef)
        {
            //LogHelper.Info($"OnAccepted : {connectedActorRef}");
            _sessions.TryAdd(connectedActorRef, 0);
        }

        protected override void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage)
        {
            LogHelper.Error($"OnDeadLetterMessage : {deadLetterMessage.Reason}");
        }

        protected override void OnDisconnected(INetworkSessionRef connectedActorRef)
        {
            _sessions.TryRemove(connectedActorRef, out _);
            //LogHelper.Info($"OnDisconnected : {connectedActorRef}");
        }
        public void Broadcast(byte[] bytes)
        {
            foreach(var session in _sessions.Keys)
            {
                session.SendAsync(bytes);
            }
        }
     }
}
