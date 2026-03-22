using Dignus.Actor.Abstractions;
using Dignus.Actor.Core;
using Dignus.Actor.Network;
using TlsActorServer.Messages;

namespace TlsActorServer.Networks
{
    internal class EchoActor(): SessionActorBase()
    {

        protected override async ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if(message is BinaryMessage rawMessage)
            {
                NetworkSession.SendAsync(rawMessage);
            }
        }
        public override void OnKill() 
        {
        }
    }
}
