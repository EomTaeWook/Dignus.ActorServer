using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Actors;
using Dignus.Actor.Network.Messages;

namespace ConsoleApp.Networks
{
    internal class PlayerActor(IActorRef transportRef) : SessionActor(transportRef)
    {
        private readonly IActorRef _transportRef = transportRef;

        protected override Task OnReceive(IActorMessage message, IActorRef sender)
        {
            if(message is BinaryMessage rawMessage)
            {
                _transportRef.Post(rawMessage);
            }
            return Task.CompletedTask;
        }
        public override void OnKill() 
        {

        }
    }
}
