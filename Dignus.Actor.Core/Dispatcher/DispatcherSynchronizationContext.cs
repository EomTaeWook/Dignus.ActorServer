// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using System.Threading;

namespace Dignus.Actor.Core.Dispatcher
{
    internal sealed class DispatcherSynchronizationContext : SynchronizationContext
    {
        private readonly ActorDispatcher _actorDispatcher;

        public DispatcherSynchronizationContext(ActorDispatcher actorDispatcher)
        {
            _actorDispatcher = actorDispatcher;
        }

        public override void Post(SendOrPostCallback sendOrPostCallback, object state)
        {
            _actorDispatcher.EnqueueContinuation(sendOrPostCallback, state);
        }
    }
}
