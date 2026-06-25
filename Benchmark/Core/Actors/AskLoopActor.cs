using Core.Messages;
using Dignus.Actor.Abstractions;
using Dignus.Actor.Core;

namespace Core.Actors
{
    internal sealed class AskLoopActor : ActorBase
    {
        private readonly IAskActorRef _target;
        private readonly AskBenchmarkState _state;

        public long Count;

        public AskLoopActor(IAskActorRef target, AskBenchmarkState state)
        {
            _target = target;
            _state = state;
        }

        protected override async ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if (message is StartAskLoopMessage)
            {
                while (_state.IsRunning)
                {
                    var response = await _target.AskAsync<AskPongMessage>(AskPingMessage.Instance, 3000);
                    if (response.Ok)
                    {
                        Count++;
                    }
                }
            }
        }
    }
}
