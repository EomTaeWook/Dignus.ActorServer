using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Actors;
using Dignus.Actor.Network.Messages;
using Dignus.Log;

namespace ConsoleApp.Networks
{
    internal class PlayerActor(IActorRef transportRef) : SessionActor(transportRef)
    {
        private readonly IActorRef _transportRef = transportRef;

        protected override ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if(message is BinaryMessage rawMessage)
            {
                //LogHelper.Info($"{rawMessage.Data.Count()}");
                _transportRef.Post(rawMessage);
            }
            return ValueTask.CompletedTask;
        }
        public override void OnKill() 
        {

        }
    }
}
