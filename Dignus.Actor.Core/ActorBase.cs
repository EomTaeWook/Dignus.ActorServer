// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Abstractions;
using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Internals;
using System;
using System.Threading.Tasks;

namespace Dignus.Actor.Core
{
    public abstract class ActorBase 
    {
        protected abstract ValueTask OnReceive(IActorMessage message, IActorRef sender);
        public virtual void OnKill()
        {
        }

        internal virtual void KillInternal()
        {
            OnKill();
        }
        public IActorRef Self => SelfActorRef;

        internal ActorDispatcher Dispatcher { get; private set; }
        internal ActorRef SelfActorRef { get; private set; }

        internal ValueTask OnReceiveInternal(IActorMessage message, IActorRef sender)
        {
            return OnReceive(message, sender);
        }

        internal void Initialize(ActorDispatcher actorDispatcher, ActorRef actorRef)
        {
            Dispatcher = actorDispatcher;
            SelfActorRef = actorRef;
        }

        public void Post(IActorRef targetRef, IActorMessage message)
        {
            targetRef.Post(message, Self);
        }

        public void VerifyContext()
        {
            ActorDispatcher actorDispatcher =
                ActorDispatcher.CurrentActorDispatcher
                ?? throw new InvalidOperationException($"Actor Dispatcher-{Dispatcher.Id} is running on ThreadPool.");

            if (actorDispatcher.Id != Dispatcher.Id)
            {
                throw new InvalidOperationException($"Actor Dispatcher-{Dispatcher.Id} vs Current Dispatcher-{actorDispatcher.Id}");
            }
        }
    }
}
