// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core;
using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network.Actors;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Tls;

namespace Dignus.Actor.Network
{
    public abstract class ActorTlsServerBase : TlsServerBase
    {
        protected abstract void OnAccepted(IActorRef connectedActorRef);
        protected abstract void OnDisconnected(IActorRef connectedActorRef);

        private readonly ActorSystem _actorSystem;

        public ActorTlsServerBase(ActorSystem actorSystem,
            ActorSessionConfiguration sessionConfiguration,
            TlsServerOptions tlsServerOptions,
            int initialSessionPoolSize = 0) :base(null, tlsServerOptions, initialSessionPoolSize)
        {
            _actorSystem = actorSystem;
        }
        protected override void OnAccepted(ISession session)
        {
            var actorRef = _actorSystem.Spawn(() => new ConnectionActor(session));
            OnAccepted(actorRef);
        }
        protected override void OnDisconnected(ISession session)
        {
            OnDisconnected(null);
        }

        protected override void OnHandshaking(ISession session)
        {   
        }
    }
}
