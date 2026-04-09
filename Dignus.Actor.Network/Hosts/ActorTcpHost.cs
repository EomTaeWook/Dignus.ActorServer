// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Internals;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Tcp;

namespace Dignus.Actor.Network.Hosts
{
    internal class ActorTcpHost(IActorHostHandler handler,
        SessionConfiguration sessionConfiguration
        ) : TcpServerBase(sessionConfiguration)
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
