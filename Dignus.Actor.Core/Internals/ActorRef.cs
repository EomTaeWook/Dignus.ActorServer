// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Abstractions;
using Dignus.Actor.Core.Messages;
using System;

namespace Dignus.Actor.Core.Internals
{
    internal class ActorRef : IActorRef
    {
        public long Id { get => _id; }
        public string Alias { get => _alias; }

        private readonly ActorSystem _actorSystem;
        private readonly long _id;
        private readonly string _alias;

        public ActorRef(ActorSystem actorSystem, long id, string alias)
        {
            _actorSystem = actorSystem;
            _id = id;
            _alias = alias;
        }

        public void Post(IActorMessage message, IActorRef sender)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            _actorSystem.Post(_id, message, sender);
        }
        public void Post(in ActorMail actorMail)
        {
            _actorSystem.Post(_id, in actorMail);
        }

        public void Kill()
        {
            _actorSystem.Kill(_id);
        }
    }
}
