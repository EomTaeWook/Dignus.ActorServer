// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;

namespace Dignus.Actor.Core
{
    internal interface IActorRefProvider
    {
        bool TryGetActorRef(int id, out IActorRef actorRef);

        bool TryGetActorRef(string alias, out IActorRef actorRef);
    }
}
