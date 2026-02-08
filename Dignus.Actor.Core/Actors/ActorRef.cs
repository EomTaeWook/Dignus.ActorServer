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

    internal class ActorRef : IActorRef
    {
        private readonly long _id;
        private ActorSystem _actorSystem;
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
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _actorSystem.Post(_id, message, sender);
        }
    }
}
