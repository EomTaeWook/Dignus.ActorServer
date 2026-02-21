using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Actors;
using TlsActorServer.Messages;

namespace TlsActorServer.Networks
{
    internal class EchoActor(): SessionActorBase()
    {

        protected override async ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if(message is BinaryMessage rawMessage)
            {
                NetworkSession.Send(rawMessage);
            }
        }
        public override void OnKill() 
        {
        }
    }
}
