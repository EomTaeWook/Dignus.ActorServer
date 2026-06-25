using Core.Messages;
using Dignus.Actor.Abstractions;
using Dignus.Actor.Core;

namespace Core.Actors
{
    internal sealed class AskBenchmarkActor : ActorBase
    {
        protected override ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if (message is AskPingMessage)
            {
                sender.Post(AskPongMessage.OkInstance, Self);
            }
            return ValueTask.CompletedTask;
        }
    }
}
