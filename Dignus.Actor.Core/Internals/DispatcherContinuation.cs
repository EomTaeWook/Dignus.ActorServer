// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Core.ObjectPools;
using System.Threading;

namespace Dignus.Actor.Core.Internals
{
    internal class DispatcherContinuation : IActorSchedulable
    {
        private SendOrPostCallback _sendOrPostCallback;
        private object _state;
        private readonly DispatcherContinuationPool _pool;

        internal DispatcherContinuation(DispatcherContinuationPool pool)
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
