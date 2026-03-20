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
using System.Security.Cryptography.X509Certificates;

namespace Dignus.Actor.Network
{
    public abstract class TlsServerBase<TSessionActor> 
        : IActorTlsHostHandler, IActorRefProvider
        where TSessionActor : SessionActorBase
    {
        protected abstract TSessionActor CreateSessionActor();
        protected abstract void OnAccepted(INetworkSessionRef connectedSessionRef);
        protected abstract void OnDisconnected(INetworkSessionRef disconnectedSessionRef);
        protected abstract void OnHandshakeFailed(ISession session, Exception ex);
        protected abstract void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage);
        protected virtual int GetRequestedDispatcherIndex()
        {
            return -1;
        }

        private readonly ActorSystem _actorSystem;

        private readonly ActorTlsHost _actorTlsHost;
        private readonly ActorPacketProcessor _actorPacketProcessor;
        private readonly ActorNetworkOptions _actorNetworkOptions;

        private readonly ConcurrentDictionary<long, TSessionActor> _sessionActors = new();

        private readonly bool isActorSystemOwner;

        public TlsServerBase(X509Certificate2 serverCertificate,
            IActorMessageSerializer serializer,
            IActorMessageDecoder decoder) : this(TlsServerOptions.Builder()
                                            .UseCertificate(serverCertificate)
                                            .UseSerializer(serializer)
                                            .UseDecoder(decoder).Build())
        {
        }
        public TlsServerBase(ActorSystem actorSystem,
            X509Certificate2 serverCertificate,
            IActorMessageSerializer serializer,
            IActorMessageDecoder decoder) : this(actorSystem, TlsServerOptions.Builder()
                                            .UseCertificate(serverCertificate)
                                            .UseSerializer(serializer)
                                            .UseDecoder(decoder).Build())
        {
        }

        public TlsServerBase(TlsServerOptions options) : this(null, options)
        {
        }
        public TlsServerBase(ActorSystem actorSystem, TlsServerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Network);
            ArgumentNullException.ThrowIfNull(options.Network.Decoder);
            ArgumentNullException.ThrowIfNull(options.Network.MessageSerializer);
            ArgumentNullException.ThrowIfNull(options.TlsOptions);

            if (options.Network.MailboxCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options),
                    options.Network.MailboxCapacity,
                    "options.Network.MailboxCapacity must be greater than 0.");
            }

            _actorNetworkOptions = options.Network;

            if (actorSystem != null)
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

            _actorTlsHost = new ActorTlsHost(this,
                CreateHostConfigurationFactory(options),
                options.TlsOptions);
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
        public void Dispose()
        {
            _actorSystem.OnDeadLetterDetected -= OnDeadLetterDetected;
            if (isActorSystemOwner)
            {
                _actorSystem.Dispose();
            }
        }

        private SessionConfiguration CreateHostConfigurationFactory(
            TlsServerOptions options)
        {
            return new SessionConfiguration(CreateSessionFactory, options.Network.SocketOption);
        }
        private SessionSetup CreateSessionFactory()
        {
            return new SessionSetup(_actorNetworkOptions.MessageSerializer, _actorPacketProcessor, null);
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

        void IActorTlsHostHandler.OnHandshaking(ISession session)
        {
        }

        public bool TryGetActorRef(long sessionId, out IActorRef actorRef)
        {
            actorRef = null;
            if (_sessionActors.TryGetValue(sessionId, out var session))
            {
                actorRef = session.Self;
                return true;
            }
            return false;
        }
        bool IActorRefProvider.TryGetActorRef(string alias, out IActorRef actorRef)
        {
            actorRef = null;
            return false;
        }
        void IActorTlsHostHandler.OnHandshakeFailed(ISession session, Exception ex)
        {
            OnHandshakeFailed(session, ex);
        }
        private void OnDeadLetterDetected(DeadLetterMessage obj)
        {
            OnDeadLetterMessage(obj);
        }
        public void Broadcast(byte[] bytes)
        {
            foreach (var session in _sessionActors.Values)
            {
                session.NetworkSessionRef.SendAsync(bytes);
            }
        }
    }
}
