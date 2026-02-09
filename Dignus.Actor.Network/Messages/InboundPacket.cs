using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.ObjectPools;
using System;

namespace Dignus.Actor.Network.Messages
{
    public class InboundPacket : IActorMessage
    {
        public byte[] Bytes { get; private set; }

        private readonly InboundPacketPool _pool;
        public InboundPacket()
        {
        }
        internal InboundPacket(InboundPacketPool pool)
        {
            _pool = pool;
        }
        public void SetBytes(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            Bytes = bytes;
        }
        
    }
}
