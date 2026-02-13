// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Sockets.Interfaces;

namespace Dignus.Actor.Network.Internals
{
    internal interface IActorHostHandler
    {
        void OnAccepted(ISession session);
        void OnDisconnected(ISession session);
        void OnHandshaking(ISession session);
    }
}
