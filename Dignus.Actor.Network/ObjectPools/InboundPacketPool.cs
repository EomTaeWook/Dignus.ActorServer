using Dignus.Actor.Network.Messages;
using Dignus.Framework;

namespace Dignus.Actor.Network.ObjectPools
{
    internal class InboundPacketPool
    {
        private class InnerPool(InboundPacketPool parent) : ObjectPoolBase<InboundPacket>
        {
            public override InboundPacket CreateItem()
            {
                var item = new InboundPacket(parent);
                return item;
            }
            public override void Remove(InboundPacket item)
            {
            }
        }
    }
}
