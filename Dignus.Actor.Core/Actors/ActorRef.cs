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

    internal class ActorRef : IActorRef
    {
        private ActorSystem _actorSystem;
        private readonly long _id;
        public ActorRef(ActorSystem actorSystem, long id)
        {
            _actorSystem = actorSystem;
            _id = id;
        }

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
            actorSystem.Post(_id, message, sender);
        }
    }
}
