// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Messages;
using Dignus.Sockets.Interfaces;
using System;

namespace Dignus.Actor.Network.Internals
{
    internal interface IActorHostHandler
    {
        void OnAccepted(ISession session);
        void OnDisconnected(ISession session);
    }

    internal interface IActorTlsHostHandler : IActorHostHandler
    {
        void OnHandshaking(ISession session);

        void OnHandshakeFailed(ISession session, Exception ex);
    }
}
