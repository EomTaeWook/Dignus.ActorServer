// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Dispatcher;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Dignus.Actor.Core
{
    public readonly struct ActorAwait
    {
        private readonly ActorDispatcher _dispatcher;
        internal ActorAwait(ActorDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }
        public static ActorAwaiter Join(ActorBase actorBase)
        {
            return Join(actorBase.Dispatcher);
        }
        internal static ActorAwaiter Join(ActorDispatcher dispatcher)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);

            return new ActorAwaiter(dispatcher);
        }
        public ActorAwaiter GetAwaiter()
        {
            return new ActorAwaiter(_dispatcher);
        }

        public readonly struct ActorAwaiter : ICriticalNotifyCompletion
        {
            private static readonly SendOrPostCallback _sendOrPostCallback = InvokeAction;
            private readonly ActorDispatcher _dispatcher;
            internal ActorAwaiter(ActorDispatcher dispatcher)
            {
                _dispatcher = dispatcher;
            }

            public ActorAwaiter GetAwaiter() 
            {
                return this;
            }

            private static void InvokeAction(object state)
            {
                ((Action)state).Invoke();
            }
            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }
            public bool IsCompleted
            {
                get
                {
                    return ReferenceEquals(ActorDispatcher.CurrentActorDispatcher, _dispatcher);
                }
            }
            public void UnsafeOnCompleted(Action continuation)
            {
                if (_dispatcher == null)
                {
                    throw new InvalidOperationException("ActorAwait is not initialized.");
                }
                _dispatcher.EnqueueContinuation(_sendOrPostCallback, continuation);
            }
        }
    }
}
