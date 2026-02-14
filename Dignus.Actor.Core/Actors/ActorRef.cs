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

        void Kill();
    }

    internal class ActorRef(ActorSystem actorSystem, int id) : IActorRef
    {
        public override string ToString()
        {
            return $"{base.ToString()} id: {id}";
        }
        public int Id => id;

        private ActorSystem _actorSystem = actorSystem;

        public void Post(IActorMessage message, IActorRef sender)
        {
            ArgumentNullException.ThrowIfNull(message);

            var actorSystem = _actorSystem;

            if(actorSystem == null)
            {
                return;
            }
            actorSystem.Post(id, message, sender);
        }

        public void Kill()
        {
            var actorSystem = _actorSystem;
            if (actorSystem == null)
            {
                return;
            }
            actorSystem.Kill(id);
        }
        
        public void Invalidate()
        {
            _actorSystem = null;
        }
    }
}
