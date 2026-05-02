using Dignus.Actor.Abstractions;

namespace Core.Messages
{
    internal sealed class PingMessage : IActorMessage
    {
        public static readonly PingMessage Instance = new PingMessage();
        private PingMessage()
        {
        }
    }
    internal sealed class PongMessage : IActorMessage
    {
        public static readonly PongMessage Instance = new PongMessage();
        private PongMessage()
        {
        }
    }

}
