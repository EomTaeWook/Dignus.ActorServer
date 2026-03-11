// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

namespace Dignus.Actor.Core
{
    internal interface IActorRefProvider
    {
        bool TryGetActorRef(long id, out IActorRef actorRef);

        bool TryGetActorRef(string alias, out IActorRef actorRef);
    }
}
