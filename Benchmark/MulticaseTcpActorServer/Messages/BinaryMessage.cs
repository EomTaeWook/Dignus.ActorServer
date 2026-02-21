// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Messages;
using System;

namespace Multicast.TcpActorServer.Messages
{
    public class BinaryMessage(ArraySegment<byte> bytes) : INetworkActorMessage
    {
        public ArraySegment<byte> Data { get; } = bytes;
    }
}
