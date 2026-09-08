// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Core;

namespace Dignus.Actor.Network.Internals
{
    internal interface ISessionActorRefResolver
    {
        bool TryGetActorRef(long sessionId, out IActorRef actorRef);
    }
}
