using Core.Messages;
using Dignus.Actor.Abstractions;
using Dignus.Actor.Core;

namespace Core.Actors
{
    internal sealed class AskBenchmarkActor : ActorBase
    {
        protected override ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if (message is AskPingMessage askPingMessage)
            {
                sender.Post(new AskPongMessage()
                {
                    RequestId = askPingMessage.RequestId,
                    Ok = true
                }, Self);
            }
            return ValueTask.CompletedTask;
        }
    }

}
