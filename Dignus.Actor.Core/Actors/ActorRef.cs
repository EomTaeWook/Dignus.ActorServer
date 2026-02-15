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
        void Post(in ActorMail actorMail);

        void Kill();
    }

    internal class ActorRef(ActorRunner actorRunner, int id) : IActorRef
    {
        public override string ToString()
        {
            return $"{base.ToString()} id: {id}";
        }
        public int Id => id;

        private ActorRunner _actorRunner = actorRunner;

        public void Post(IActorMessage message, IActorRef sender)
        {
            ArgumentNullException.ThrowIfNull(message);
            _actorRunner.Enqueue(message, sender);
        }
        public void Post(in ActorMail actorMail)
        {
            _actorRunner.Enqueue(actorMail);
        }

        public void Kill()
        {
            var actorRunner = _actorRunner;
            if (actorRunner == null)
            {
                return;
            }
            actorRunner.Kill();
        }
        
        public void Invalidate()
        {
            _actorRunner = null;
        }
    }
}
