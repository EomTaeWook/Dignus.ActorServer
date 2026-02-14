// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Protocol;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets;

namespace Dignus.Actor.Network.Options
{
    public sealed class ActorNetworkOptions
    {
        public IActorMessageSerializer MessageSerializer { get; set; }
        public IMessageDecoder Decoder { get; set; }
        public SocketOption SocketOption { get; set; }
        public int InitialSessionPoolSize { get; set; } = 0;
    }
}
