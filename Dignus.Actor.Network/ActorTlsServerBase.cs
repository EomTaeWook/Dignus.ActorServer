// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core;
using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network.Actors;
using Dignus.Actor.Network.Processor;
using Dignus.Actor.Network.Serializer;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Tls;
using System;

namespace Dignus.Actor.Network
{
    public abstract class ActorTlsServerBase<TSessionActor> : TlsServerBase where TSessionActor : SessionActor
    {
        protected abstract TSessionActor CreateSessionActor(IActorRef transportActorRef);
        protected abstract void OnSessionAccepted(TSessionActor connectedActorRef);
        protected abstract void OnDisconnected(IActorRef connectedActorRef);

        private readonly ActorSystem _actorSystem;

        private readonly ActorPacketProcessor _processor;
        private IActorMessageSerializer _actorMessageSerializer;


        public ActorTlsServerBase(ActorSystem actorSystem,
            IActorMessageSerializer actorMessageSerializer,
            ActorPacketProcessor actorPacketProcessor,
            TlsServerOptions tlsServerOptions,
            SocketOption socketOption = null,
            int initialSessionPoolSize = 0) :base(CreateSessionConfigurationFactory(actorMessageSerializer, actorPacketProcessor, socketOption), tlsServerOptions, initialSessionPoolSize)
        {
            ArgumentNullException.ThrowIfNull(actorSystem);
            ArgumentNullException.ThrowIfNull(actorMessageSerializer);
            ArgumentNullException.ThrowIfNull(actorPacketProcessor);

            _actorMessageSerializer = actorMessageSerializer;
            _actorSystem = actorSystem;
            _processor = actorPacketProcessor;
        }
        protected override void OnAccepted(ISession session)
        {
            IActorRef transportActorRef = _actorSystem.Spawn(() => new TransportActor(session, _actorMessageSerializer));

            var sessionActor = CreateSessionActor(transportActorRef);

            OnSessionAccepted(sessionActor);
        }


        protected override void OnDisconnected(ISession session)
        {
            
        }

        protected override void OnHandshaking(ISession session)
        {   
        }

        private static SessionSetupFactoryDelegate CreateSessionSetupFactory(IActorMessageSerializer serializer, ActorPacketProcessor processor)
        {
            return () => { return new SessionSetup(serializer, processor, null); };
        }
        private static SessionConfiguration CreateSessionConfigurationFactory(
            IActorMessageSerializer serializer,
            ActorPacketProcessor processor,
            SocketOption socketOption = null)
        {
            return new SessionConfiguration(CreateSessionSetupFactory(serializer, processor), socketOption);
        }
    }
}
