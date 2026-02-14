// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network.Protocol;
using Dignus.Collections;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Processing;
using System;
using System.Threading.Tasks;

namespace Dignus.Actor.Network.Internals
{
    internal class ActorPacketProcessor(IMessageDecoder decoder,
        IActorRefProvider actorRefProvider) : PacketProcessor
    {
        protected override Task ProcessPacketAsync(ISession session, ArraySegment<byte> packet)
        {
            if (!actorRefProvider.TryGetActorRef(session.Id, out IActorRef actorRef))
            {
                return Task.CompletedTask;
            }
            var message = decoder.Deserialize(packet);

            actorRef.Post(message);

            return Task.CompletedTask;
        }
        protected override bool TakeReceivedPacket(ISession session, ArrayQueue<byte> buffer, out ArraySegment<byte> packet, out int consumedBytes)
        {
            return decoder.TryFrame(session, buffer, out packet, out consumedBytes);
        }
    }
}
