// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

namespace Dignus.Actor.Core
{
    internal interface IActorRefResolver
    {
        bool TryGetActorRef(long id, out IActorRef actorRef);

        bool TryGetActorRef(string alias, out IActorRef actorRef);
    }
}
