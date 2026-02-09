using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Messages;
using Dignus.Actor.Network.Serializer;
using Dignus.Collections;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Processing;
using System;
using System.Threading.Tasks;

namespace Dignus.Actor.Network.Processor
{
    public abstract class ActorRoutingPacketProcessor : PacketProcessor
    {
        protected abstract IActorRef GetActorRef(ISession session);
        protected abstract bool TakeReceivedPacket(IActorRef actorRef,
            ArrayQueue<byte> buffer,
            out ArraySegment<byte> packet,
            out int consumedBytes);

        protected override Task ProcessPacketAsync(ISession session, ArraySegment<byte> packet)
        {
            var actorRef = GetActorRef(session);

            if (actorRef == null)
            {
                return Task.CompletedTask;
            }

            var copy = new byte[packet.Count];
            Buffer.BlockCopy(packet.Array!, packet.Offset, copy, 0, packet.Count);

            //actorRef.Post(new InboundPacket(copy));

            //IActorMessage message = _serializer.Deserialize(packet);
            //if (message != null)
            //{
                
            //    actorRef.Post(message);
            //}
            return Task.CompletedTask;
        }

        protected override bool TakeReceivedPacket(ISession session, ArrayQueue<byte> buffer, out ArraySegment<byte> packet, out int consumedBytes)
        {
            return TakeReceivedPacket(GetActorRef(session), buffer, out packet, out consumedBytes);
        }
    }
}
