// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Internal;
using Dignus.Actor.Core.Messages;
using System;

namespace Dignus.Actor.Core.Actors
{
    public interface IActorRef : IDisposable
    {
        void Post(IActorMessage message, IActorRef sender = null);
    }

    internal class ActorRef(ActorSystem actorSystem, long id) : IActorRef
    {
        public void Dispose()
        {
            actorSystem = null;
        }

        public void Post(IActorMessage message, IActorRef sender = null)
        {
            ArgumentNullException.ThrowIfNull(message);

            actorSystem.Post(id, message, sender);
        }
    }
}
