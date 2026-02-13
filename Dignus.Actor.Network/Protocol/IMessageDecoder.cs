// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Messages;
using Dignus.Collections;
using Dignus.Sockets.Interfaces;
using System;

namespace Dignus.Actor.Network.Protocol
{
    public interface IMessageDecoder
    {
        public IActorMessage Deserialize(ReadOnlySpan<byte> packet);

        bool TryFrame(ISession session, ArrayQueue<byte> buffer, out ArraySegment<byte> packet, out int consumedBytes);
    }
}
