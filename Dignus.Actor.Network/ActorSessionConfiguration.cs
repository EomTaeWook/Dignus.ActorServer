// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Processor;
using Dignus.Actor.Network.Serializer;
using Dignus.Sockets;
using System;
namespace Dignus.Actor.Network
{
    public class ActorSessionConfiguration
    {
        internal SessionConfiguration SessionConfiguration { get; init; }
        internal ActorPacketProcessor ActorPacketProcessor { get; init; }
        internal IActorMessageSerializer Serializer { get; init; }
        public ActorSessionConfiguration(IActorMessageSerializer serializer,
            ActorPacketProcessor actorPacketProcessor,
            SocketOption socketOption= null)
        {
            ArgumentNullException.ThrowIfNull(serializer);
            ArgumentNullException.ThrowIfNull(actorPacketProcessor);

            Serializer = serializer;
            ActorPacketProcessor = actorPacketProcessor;
            SessionConfiguration = new SessionConfiguration(SessionSetupFactory, socketOption);
        }

        public SessionSetup SessionSetupFactory()
        {
            return new SessionSetup(Serializer,
                ActorPacketProcessor, 
                null);
        }
    }
}
