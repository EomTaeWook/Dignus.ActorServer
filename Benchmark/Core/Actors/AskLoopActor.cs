using Core.Messages;
using Dignus.Actor.Abstractions;
using Dignus.Actor.Core;

namespace Core.Actors
{
    internal sealed class AskLoopActor(IAskActorRef target, AskBenchmarkState state) : ActorBase
    {
        public long Count;

        protected override async ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if (message is StartAskLoopMessage)
            {
                while (state.IsRunning)
                {
                    var response = await target.AskAsync<AskPongMessage>(AskPingMessage.Instance, 3000);
                    if (response.Ok)
                    {
                        Count++;
                    }
                }
            }
        }
    }
}
