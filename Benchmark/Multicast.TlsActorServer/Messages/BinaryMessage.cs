// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.


using Dignus.Actor.Network.Messages;
using System;

namespace Multicast.TlsActorServer.Messages
{
    public class BinaryMessage(ArraySegment<byte> bytes) : INetworkActorMessage
    {
        public ArraySegment<byte> Data { get; } = bytes;
    }
}
