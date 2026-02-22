// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Internals;
using Dignus.Actor.Core.ObjectPools;
using Dignus.Collections;
using System;
using System.Threading;

namespace Dignus.Actor.Core.Dispatcher
{
    internal sealed class ActorDispatcher : IDisposable
    {
        [ThreadStatic]
        public static ActorDispatcher CurrentActorDispatcher;

        public int Id => _dispatcherId;

        private readonly int _dispatcherId;

        private readonly SynchronizedArrayQueue<IActorSchedulable> _scheduledActors = [];

        private readonly ActorYieldTaskPool _yieldTaskPool = new();
        private volatile bool _isStopped;
        private readonly DispatcherSynchronizationContext _synchronizationContext;
        private readonly Thread _workerThread;

        private readonly SemaphoreSlim _signal = new(0, int.MaxValue);
        private int _signalPending = 0;

        public ActorDispatcher(int dispatcherId)
        {
            _dispatcherId = dispatcherId;
            _synchronizationContext = new DispatcherSynchronizationContext(this);

            _workerThread = new Thread(ProcessScheduledActors)
            {
                IsBackground = true,
                Name = $"ActorDispatcher-{_dispatcherId}",
                Priority = ThreadPriority.AboveNormal
            };
        }

        public void Start()
        {
            _workerThread.Start();
        }

        private void ProcessScheduledActors()
        {
            CurrentActorDispatcher = this;
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

            while (true)
            {
                _signal.Wait();

                if (_isStopped) 
                {
                    break;
                }

                Interlocked.Exchange(ref _signalPending, 0);

                while (_scheduledActors.TryRead(out IActorSchedulable actorSchedulable))
                {
                    actorSchedulable.Execute();

                    if (_isStopped)
                    {
                        break;
                    }
                }

                if (_isStopped)
                {
                    break;
                }
            }

            CurrentActorDispatcher = null;
            SynchronizationContext.SetSynchronizationContext(null);
        }

        public void Dispose()
        {
            _isStopped = true;
            _signal.Release();

            if (Thread.CurrentThread != _workerThread)
            {
                _workerThread.Join();
            }
            _signal.Dispose();
        }

        internal void Schedule(IActorSchedulable actorSchedulable)
        {
            if(_isStopped == true)
            {
                return;
            }

            _scheduledActors.Add(actorSchedulable);
            if (Interlocked.CompareExchange(ref _signalPending, 1, 0) == 0)
            {
                _signal.Release();
            }
        }

        internal void EnqueueContinuation(SendOrPostCallback sendOrPostCallback, object state)
        {
            ActorYieldTask yieldTask = _yieldTaskPool.Pop();
            yieldTask.Set(sendOrPostCallback, state);
            Schedule(yieldTask);
        }
    }
}

