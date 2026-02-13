// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Serialization;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Processing;
using System;
using System.Collections.Generic;

namespace Dignus.Actor.Network
{
    public class ActorSessionSetup
    {
        public SessionSetup SessionSetup => _sessionSetup;

        private readonly SessionSetup _sessionSetup;
        public ActorSessionSetup(IActorMessageSerializer serializer,
            PacketProcessor packetProcessor,
            ICollection<ISessionComponent> components)
        {
            ArgumentNullException.ThrowIfNull(serializer);
            ArgumentNullException.ThrowIfNull(packetProcessor);

            _sessionSetup = new SessionSetup(serializer, packetProcessor, components);
        }
    }
}
