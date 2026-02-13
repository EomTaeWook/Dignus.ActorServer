using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Actors;

namespace ConsoleApp.Networks
{
    internal class PlayerActor : SessionActor
    {
        public PlayerActor(IActorRef transportRef) : base(transportRef)
        {
        }

        protected override Task OnReceive(IActorMessage message, IActorRef sender = null)
        {
            return Task.CompletedTask;
        }
    }
}
