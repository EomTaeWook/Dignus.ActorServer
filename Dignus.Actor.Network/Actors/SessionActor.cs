// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;

namespace Dignus.Actor.Network.Actors
{
    public abstract class SessionActor(IActorRef transportRef
        ) : ActorBase
    {
        protected IActorRef TransportRef => transportRef;





    }
}
