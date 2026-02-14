// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Internals;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;

namespace Dignus.Actor.Network.Hosts
{
    internal class ActorTcpHost(IActorHostHandler handler,
        SessionConfiguration sessionConfiguration,
        int initialSessionPoolSize = 0) : ServerBase(sessionConfiguration,
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
    }
}
