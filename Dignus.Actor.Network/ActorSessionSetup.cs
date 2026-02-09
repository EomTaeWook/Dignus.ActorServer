// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Processor;
using Dignus.Collections;
using Dignus.Sockets.Interfaces;
using System;
using System.Collections.Generic;

namespace Dignus.Actor.Network
{
    public class ActorSessionSetup
    {
        public IPacketSerializer PacketSerializer { get; private set; }

        public ActorRoutingPacketProcessor PacketProcessor { get; private set; }

        public ArrayQueue<ISessionComponent> SessionComponents { get; private set; } = new ArrayQueue<ISessionComponent>();

        public ActorSessionSetup(IPacketSerializer serializer, ActorRoutingPacketProcessor packetProcessor, ICollection<ISessionComponent> components)
        {
            ArgumentNullException.ThrowIfNull(serializer);

            ArgumentNullException.ThrowIfNull(packetProcessor);

            PacketSerializer = serializer;

            PacketProcessor = packetProcessor;

            if (components != null)
            {
                SessionComponents.AddRange(components);
            }
        }
    }
}
