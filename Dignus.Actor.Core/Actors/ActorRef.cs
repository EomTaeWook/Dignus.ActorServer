// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Messages;
using System;

namespace Dignus.Actor.Core.Actors
{
    public interface IActorRef
    {
        void Post(IActorMessage message, IActorRef sender = null);
    }

    internal class ActorRef(ActorSystem actorSystem, int id) : IActorRef
    {
        private ActorSystem _actorSystem = actorSystem;

        public void Dispose()
        {
            _actorSystem = null;
        }

        public void Post(IActorMessage message, IActorRef sender = null)
        {
            ArgumentNullException.ThrowIfNull(message);

            var actorSystem = _actorSystem;

            if(actorSystem == null)
            {
                return;
            }
            actorSystem.Post(id, message, sender);
        }
    }
}
