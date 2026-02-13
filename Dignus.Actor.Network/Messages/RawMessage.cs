using Dignus.Actor.Core.Messages;

namespace Dignus.Actor.Network.Messages
{
    public class RawMessage(byte[] bytes) : IActorMessage
    {
        public byte[] Data { get; } = bytes;
    }
}
