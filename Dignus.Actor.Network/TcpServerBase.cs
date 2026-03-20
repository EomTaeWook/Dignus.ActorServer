// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core;
using Dignus.Actor.Core.DeadLetter;
using Dignus.Actor.Network.Codec;
using Dignus.Actor.Network.Hosts;
using Dignus.Actor.Network.Internals;
using Dignus.Actor.Network.Options;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace Dignus.Actor.Network
{
    public abstract class TcpServerBase<TSessionActor> : IActorHostHandler, IActorRefProvider
        where TSessionActor : SessionActorBase
    {
        protected abstract TSessionActor CreateSessionActor();
        protected abstract void OnAccepted(INetworkSessionRef connectedActorRef);
        protected abstract void OnDisconnected(INetworkSessionRef disconnectedSessionRef);
        protected abstract void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage);
        protected virtual int GetRequestedDispatcherIndex()
        {
            return -1;
        }

        private readonly ConcurrentDictionary<long, TSessionActor> _sessionActors = new();

        private readonly ActorNetworkOptions _actorNetworkOptions;
        private readonly ActorSystem _actorSystem;
        private readonly ActorPacketProcessor _actorPacketProcessor;

        private readonly ActorTcpHost _actorTcpHost;
        private readonly bool isActorSystemOwner;
        public TcpServerBase(ServerOptions options) : this(null, options)
        {
        }
        public TcpServerBase(ActorSystem actorSystem, ServerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Network);
            ArgumentNullException.ThrowIfNull(options.Network.Decoder);
            ArgumentNullException.ThrowIfNull(options.Network.MessageSerializer);

            if (options.Network.MailboxCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options),
                    options.Network.MailboxCapacity,
                    "options.Network.MailboxCapacity must be greater than 0.");
            }
            _actorNetworkOptions = options.Network;

            if(actorSystem != null)
            {
                _actorSystem = actorSystem;
            }
            else
            {
                isActorSystemOwner = true;
                options.ActorSystem ??= new ActorSystemOptions();
                _actorSystem = new ActorSystem(options.ActorSystem.DispatcherThreadCount);
            }

            _actorSystem.OnDeadLetterDetected += OnDeadLetterDetected;

            _actorPacketProcessor = new ActorPacketProcessor(_actorNetworkOptions.Decoder, this);

            _actorTcpHost = new ActorTcpHost(this,
                CreateHostConfigurationFactory(options),
                options.InitialSessionPoolSize);
        }

        private void OnDeadLetterDetected(DeadLetterMessage obj)
        {
            OnDeadLetterMessage(obj);
        }

        public TcpServerBase(IActorMessageSerializer serializer,
            IActorMessageDecoder decoder) : this(ServerOptions.Builder()
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

        public bool TryGetActorRef(long sessionId, out IActorRef actorRef)
        {
            actorRef = null;
            if(_sessionActors.TryGetValue(sessionId, out var sessionActor))
            {
                actorRef = sessionActor.Self;
                return true;
            }
            return false;
        }

        bool IActorRefProvider.TryGetActorRef(string alias, out IActorRef actorRef)
        {
            actorRef = null;
            return false;
        }

        void IActorHostHandler.OnAccepted(ISession session)
        {
            var sessionActor = CreateSessionActor();

            var dispatcherIndex = GetRequestedDispatcherIndex();

            if (dispatcherIndex < 0)
            {
                sessionActor = _actorSystem.SpawnWithAutoDispatcher(sessionActor,
                    null,
                    _actorNetworkOptions.MailboxCapacity);
            }
            else
            {
                sessionActor = _actorSystem.SpawnWithDispatcher(sessionActor,
                    dispatcherIndex,
                    null,
                    _actorNetworkOptions.MailboxCapacity);
            }
            _sessionActors[session.Id] = sessionActor;

            var networkSessionRef = new NetworkSessionRef(sessionActor.SelfActorRef,
                                    session,
                                    _actorNetworkOptions.MessageSerializer);

            sessionActor.Initialize(networkSessionRef);

            OnAccepted(networkSessionRef);
        }

        void IActorHostHandler.OnDisconnected(ISession session)
        {
            if (_sessionActors.TryRemove(session.Id, out var sessionActor))
            {
                if(sessionActor.NetworkSessionRef != null)
                {
                    OnDisconnected(sessionActor.NetworkSessionRef);
                }
                sessionActor.SelfActorRef.Kill();
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
        public void Dispose()
        {
            _actorSystem.OnDeadLetterDetected -= OnDeadLetterDetected;
            if (isActorSystemOwner)
            {
                _actorSystem.Dispose();
            }
        }
        public void Broadcast(byte[] bytes)
        {
            foreach(var session in _sessionActors.Values)
            {
                session.NetworkSessionRef.SendAsync(bytes);
            }
        }
    }
}
