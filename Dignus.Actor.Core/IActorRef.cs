// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Abstractions;
using Dignus.Actor.Core.Messages;

namespace Dignus.Actor.Core
{
    public interface IActorRef
    {
        void Post(IActorMessage message, IActorRef sender = null);
        void Post(in ActorMail actorMail);
        void Kill();
    }
}
