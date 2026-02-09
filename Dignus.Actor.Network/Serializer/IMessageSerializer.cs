// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Messages;
using Dignus.Sockets.Interfaces;
using System;

namespace Dignus.Actor.Network.Serializer
{
    public interface IMessageSerializer : IPacketSerializer
    {
        ArraySegment<byte> MakeSendBuffer(IActorMessage packet);
    }
}
