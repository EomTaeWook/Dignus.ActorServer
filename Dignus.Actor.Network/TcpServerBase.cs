// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core;
using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Actors;
using Dignus.Actor.Network.Hosts;
using Dignus.Actor.Network.Internals;
using Dignus.Actor.Network.Options;
using Dignus.Actor.Network.Protocol;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace Dignus.Actor.Network
{
    public abstract class TcpServerBase<TSessionActor> : IActorHostHandler, IActorRefProvider
        where TSessionActor : SessionActor
    {
        protected abstract TSessionActor CreateSessionActor(IActorRef transportActorRef);
        protected abstract void OnAccepted(IActorRef connectedActorRef);
        protected abstract void OnDisconnected(IActorRef connectedActorRef);
        protected abstract void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage);

        private readonly ConcurrentDictionary<int, IActorRef> _sessionActors = new();

        private readonly ActorNetworkOptions _actorNetworkOptions;
        private readonly ActorSystem _actorSystem;
        private readonly ActorPacketProcessor _actorPacketProcessor;

        private readonly ActorTcpHost _actorTcpHost;
        public TcpServerBase(ServerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Network);
            ArgumentNullException.ThrowIfNull(options.Network.Decoder);
            ArgumentNullException.ThrowIfNull(options.Network.MessageSerializer);

            _actorNetworkOptions = options.Network;

            options.ActorSystem ??= new ActorSystemOptions();

            _actorSystem = new ActorSystem(options.ActorSystem.DispatcherThreadCount);
            _actorSystem.OnDeadLetterDetected += OnDeadLetterDetected;

            _actorPacketProcessor = new ActorPacketProcessor(_actorNetworkOptions.Decoder, this);

            _actorTcpHost = new ActorTcpHost(this,
                CreateHostConfigurationFactory(options),
                options.Network.InitialSessionPoolSize);
        }
        ~TcpServerBase() 
        {
            _actorSystem.OnDeadLetterDetected -= OnDeadLetterDetected;
        }

        private void OnDeadLetterDetected(DeadLetterMessage obj)
        {
            OnDeadLetterMessage(obj);
        }

        public TcpServerBase(IActorMessageSerializer serializer,
            IMessageDecoder decoder) : this(ServerOptions.Builder()
                                        .UseSerializer(serializer)
                                        .UseDecoder(decoder)
                                        .Build())
        {
        }
        private SessionConfiguration CreateHostConfigurationFactory(
            ServerOptions options)
        {
            return new SessionConfiguration(CreateSessionFactory, options.Network.SocketOption);
        }
        private SessionSetup CreateSessionFactory()
        {
            return new SessionSetup(_actorNetworkOptions.MessageSerializer, _actorPacketProcessor, null);
        }

        public bool TryGetActorRef(int sessionId, out IActorRef actorRef)
        {
            return _sessionActors.TryGetValue(sessionId, out actorRef);
        }

        bool IActorRefProvider.TryGetActorRef(string alias, out IActorRef actorRef)
        {
            actorRef = null;
            return false;
        }

        void IActorHostHandler.OnAccepted(ISession session)
        {
            IActorRef transportRef = _actorSystem.Spawn(() => new TransportActor(session));

            IActorRef sessionRef = _actorSystem.Spawn(() => 
            {
                var sessionActor = CreateSessionActor(transportRef);
                sessionActor.Initialize(_actorNetworkOptions.MessageSerializer);
                return sessionActor;
            });

            _sessionActors[session.Id] = sessionRef;

            OnAccepted(sessionRef);
        }

        void IActorHostHandler.OnDisconnected(ISession session)
        {
            if (_sessionActors.TryRemove(session.Id, out var sessionRef))
            {
                sessionRef.Kill();
                OnDisconnected(sessionRef);
            }
        }

        public void Start(string ip, int port, int backlog)
        {
            _actorTcpHost.Start(ip, port, backlog);
        }
        public void Start(int port, int backlog = 200)
        {
            _actorTcpHost.Start(port, backlog);
        }
        public void Start(IPEndPoint ipEndPoint, int backlog)
        {
            _actorTcpHost.Start(ipEndPoint, backlog);
        }
        public void Close()
        {
            _actorTcpHost.Close();
        }

        public ICollection<IActorRef> GetAllSessionActors()
        {
            return _sessionActors.Values;
        }
    }
}
