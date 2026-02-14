// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Internals;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Tls;

namespace Dignus.Actor.Network.Hosts
{
    internal class ActorTlsHost(IActorTlsHostHandler handler,
        SessionConfiguration sessionConfiguration,
        TlsServerOptions tlsServerOptions,
        int initialSessionPoolSize = 0) : TlsServerBase(sessionConfiguration,
            tlsServerOptions,
            initialSessionPoolSize)
    {
        protected override void OnAccepted(ISession session)
        {
            handler.OnAccepted(session);
        }

        protected override void OnDisconnected(ISession session)
        {
            handler.OnDisconnected(session);
        }

        protected override void OnHandshaking(ISession session)
        {
            handler.OnHandshaking(session);
        }
    }
}
