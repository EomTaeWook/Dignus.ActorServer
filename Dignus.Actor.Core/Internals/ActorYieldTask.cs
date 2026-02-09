// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.ObjectPools;
using System.Threading;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Internals
{
    internal class ActorYieldTask : IActorSchedulable
    {
        private SendOrPostCallback _callback;
        private object _state;
        private readonly ActorYieldTaskPool _pool;

        internal ActorYieldTask(ActorYieldTaskPool pool)
        {
            _pool = pool;
        }
        public void Set(SendOrPostCallback callback, object state)
        {
            _callback = callback;
            _state = state;
        }

        public void Clear()
        {
            _callback = null;
            _state = null;
        }
        public Task ExecuteAsync()
        {
            try
            {
                _callback?.Invoke(_state);
            }
            finally
            {
                _pool.Push(this);
            }
            return Task.CompletedTask;
        }
    }
}
