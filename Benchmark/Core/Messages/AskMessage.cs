using Dignus.Actor.Abstractions;

namespace Core.Messages
{
    internal sealed class AskPingMessage : IActorMessage
    {
        public static readonly AskPingMessage Instance = new AskPingMessage();
        public long RequestId { get; set; }
    }
    internal sealed class AskPongMessage : IActorMessage
    {
        public static readonly AskPongMessage OkInstance = new AskPongMessage { Ok = true };
        public long RequestId { get; set; }
        public bool Ok { get; set; }
    }
    internal sealed class StartAskLoopMessage : IActorMessage
    {
    }
}
