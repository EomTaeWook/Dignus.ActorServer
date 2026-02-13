// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core;
using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network.Actors;
using Dignus.Actor.Network.Internals;
using Dignus.Actor.Network.Protocol;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Dignus.Actor.Network
{
    public abstract class ActorTlsServerBase<TSessionActor> : IActorHostHandler, IActorRefProvider where TSessionActor : SessionActor
    {
        protected abstract TSessionActor CreateSessionActor(IActorRef transportActorRef);
        protected abstract void OnAccepted(IActorRef connectedActorRef);
        protected abstract void OnDisconnected(IActorRef connectedActorRef);

        private readonly ActorSystem _actorSystem;

        private readonly ActorTlsHost _actorTlsHost;
        private readonly ActorPacketProcessor _actorPacketProcessor;
        private readonly ActorNetworkOptions _actorNetworkOptions;

        private readonly ConcurrentDictionary<int, IActorRef> _sessionActors = new();

        public ActorTlsServerBase(X509Certificate2 serverCertificate,
            IActorMessageSerializer actorMessageSerializer,
            IMessageDecoder decoder) : this(ActorTlsServerOptions.Builder()
                                            .UseCertificate(serverCertificate)
                                            .UseSerializer(actorMessageSerializer)
                                            .UseDecoder(decoder).Build())
        {
        }

        public ActorTlsServerBase(ActorTlsServerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Network);
            ArgumentNullException.ThrowIfNull(options.Network.Decoder);
            ArgumentNullException.ThrowIfNull(options.Network.MessageSerializer);
            ArgumentNullException.ThrowIfNull(options.TlsOptions);

            _actorNetworkOptions = options.Network;

            if(options.ActorSystem == null)
            {
                options.ActorSystem = new ActorSystemOptions();
            }

            _actorSystem = new ActorSystem(options.ActorSystem.DispatcherThreadCount);

            _actorPacketProcessor = new ActorPacketProcessor(_actorNetworkOptions.Decoder, this);

            _actorTlsHost = new ActorTlsHost(this,
                CreateHostConfigurationFactory(options),
                options.TlsOptions,
                options.Network.InitialSessionPoolSize);
            
        }
        void IActorHostHandler.OnAccepted(ISession session)
        {
            IActorRef transportRef = _actorSystem.Spawn(() => new TransportActor(session));

            IActorRef sessionRef = _actorSystem.Spawn(() => CreateSessionActor(transportRef));

            _sessionActors[session.Id] = sessionRef;

            OnAccepted(sessionRef);
        }

        public void Start(string ip, int port, int backlog)
        {
            _actorTlsHost.Start(ip, port, backlog);
        }
        public void Start(int port, int backlog = 200)
        {
            _actorTlsHost.Start(port, backlog);
        }
        public void Start(IPEndPoint ipEndPoint, int backlog)
        {
            _actorTlsHost.Start(ipEndPoint, backlog);
        }
        public void Close() 
        {
            _actorTlsHost.Close();
        }

        private SessionConfiguration CreateHostConfigurationFactory(
            ActorTlsServerOptions options)
        {
            return new SessionConfiguration(CreateSessionFactory, options.Network.SocketOption);
        }
        private SessionSetup CreateSessionFactory()
        {
            return new SessionSetup(_actorNetworkOptions.MessageSerializer, _actorPacketProcessor, null);
        }

        void IActorHostHandler.OnDisconnected(ISession session)
        {
            if (_sessionActors.TryRemove(session.Id, out var sessionRef))
            {
                OnDisconnected(sessionRef);
            }
        }

        void IActorHostHandler.OnHandshaking(ISession session)
        {
        }

        public IActorRef GetActorRef(int sessionId)
        {
            if(_sessionActors.TryGetValue(sessionId, out var actorRef))
            {
                return actorRef;
            }

            return null;
        }
    }
}
