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

    internal class ActorRef(ActorSystem actorSystem,
        int id,
        string alias) : IActorRef
    {
        public int Id { get => id; }
        public string Alias { get => alias; }

        public void Post(IActorMessage message, IActorRef sender)
        {
            ArgumentNullException.ThrowIfNull(message);
            actorSystem.Post(id, message, sender);
        }
        public void Post(in ActorMail actorMail)
        {
            actorSystem.Post(id, in actorMail);
        }

        public void Kill()
        {
            actorSystem.Kill(id);
        }
    }
}
