// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.ObjectPools;
using System.Threading;

namespace Dignus.Actor.Core.Internals
{
    internal class ActorYieldTask : IActorSchedulable
    {
        private SendOrPostCallback _sendOrPostCallback;
        private object _state;
        private readonly ActorYieldTaskPool _pool;

        internal ActorYieldTask(ActorYieldTaskPool pool)
        {
            _pool = pool;
        }
        public void Set(SendOrPostCallback callback, object state)
        {
            _sendOrPostCallback = callback;
            _state = state;
        }

        public void Recycle()
        {
            _sendOrPostCallback = null;
            _state = null;

            _pool.Push(this);
        }
        public void Execute()
        {
            _sendOrPostCallback?.Invoke(_state);
            Recycle();
        }
    }
}
