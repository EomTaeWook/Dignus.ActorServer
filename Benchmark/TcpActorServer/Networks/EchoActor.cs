using Dignus.Actor.Core;
using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Actors;
using Dignus.Actor.Network.Messages;

namespace TcpActorServer.Networks
{
    internal class EchoActor(IActorRef transportRef) : SessionActor(transportRef)
    {
        private readonly IActorRef _transportRef = transportRef;

        protected override async ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if(message is BinaryMessage rawMessage)
            {
                _transportRef.Post(rawMessage);
            }
        }
        public override void OnKill() 
        {

        }
    }
}
