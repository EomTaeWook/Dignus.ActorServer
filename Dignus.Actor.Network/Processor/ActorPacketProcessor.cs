using Dignus.Actor.Network.Serializer;
using Dignus.Collections;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Processing;
using System;
using System.Threading.Tasks;

namespace Dignus.Actor.Network.Processor
{
    public abstract class ActorPacketProcessor(IActorMessageSerializer serializer
        ) : PacketProcessor
    {
        protected override Task ProcessPacketAsync(ISession session, ArraySegment<byte> packet)
        {
            var message = serializer.Deserialize(packet);

            return Task.CompletedTask;
        }
    }
}
