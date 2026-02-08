// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using System.Threading;

namespace Dignus.Actor.Core.Dispatcher
{
    internal sealed class DispatcherSynchronizationContext(ActorDispatcher actorDispatcher) : SynchronizationContext
    {
        public override void Post(SendOrPostCallback sendOrPostCallback, object state)
        {
            actorDispatcher.EnqueueContinuation(sendOrPostCallback, state);
        }
    }
}
