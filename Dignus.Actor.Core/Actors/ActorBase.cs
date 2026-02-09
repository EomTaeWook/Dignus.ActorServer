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
        public IActorRef Self { get; internal set; }

        protected abstract Task OnReceive(IActorMessage message, IActorRef sender = null);

        internal ActorDispatcher _actorDispatcher;
        internal Task OnReceiveInternal(IActorMessage message, IActorRef sender)
        {
            return OnReceive(message, sender);
        }
        internal void SetDispatcher(ActorDispatcher dispatcher)
        {
            _actorDispatcher = dispatcher;
        }
    }
}
