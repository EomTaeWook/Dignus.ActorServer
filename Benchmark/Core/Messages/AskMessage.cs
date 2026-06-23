using Dignus.Actor.Abstractions;

namespace Core.Messages
{
    internal sealed class AskPingMessage : IActorMessage
    {
        public long RequestId { get; set; }
    }
    internal sealed class AskPongMessage : IActorMessage
    {
        public long RequestId { get; set; }
        public bool Ok { get; set; }
    }
    internal sealed class StartAskLoopMessage : IActorMessage
    {
    }
}
