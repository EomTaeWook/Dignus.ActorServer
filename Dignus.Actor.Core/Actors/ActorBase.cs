// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Messages;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Actors
{
    public abstract class ActorBase 
    {
        public IActorRef Self => SelfActorRef;

        internal ActorRef SelfActorRef { get; private set; }

        protected abstract ValueTask OnReceive(IActorMessage message, IActorRef sender);

        internal ActorDispatcher Dispatcher { get; private set; }

        internal ValueTask OnReceiveInternal(IActorMessage message, IActorRef sender)
        {
            return OnReceive(message, sender);
        }
        internal virtual void Cleanup()
        {
        }
        internal void Bind(ActorDispatcher actorDispatcher, ActorRef actorRef)
        {
            Dispatcher = actorDispatcher;
            SelfActorRef = actorRef;
        }

        public void Post(IActorRef targetRef, IActorMessage message)
        {
            targetRef.Post(message, Self);
        }
        
        public virtual void OnKill() { }
    }
}
