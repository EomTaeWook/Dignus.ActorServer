using Dignus.Actor.Abstractions;
using Dignus.Actor.Core;
using Dignus.Actor.Network;
using Multicast.TlsActorServer.Messages;

namespace Multicast.TlsActorServer.Networks
{
    internal class EchoActor : SessionActorBase
    {
        public EchoActor()
        {
        }

        protected override async ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if(message is BinaryMessage rawMessage)
            {
                NetworkSession.Send(rawMessage);
            }
        }
        protected override void OnKill() 
        {

        }
    }
}
