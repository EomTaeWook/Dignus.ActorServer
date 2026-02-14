// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.ObjectPools;
using Dignus.Collections;
using System;
using System.Threading;

namespace Dignus.Actor.Core.Dispatcher
{
    internal class ActorDispatcher : IDisposable
    {
        [ThreadStatic]
        public static ActorDispatcher CurrentActorDispatcher;
        public int Id => _dispatcherId;
        public ActorMailPool MailPool => _actorMailPool;

        private readonly int _dispatcherId;

        private readonly ManualResetEventSlim _signal = new(false);

        private readonly Thread _workerThread;
        private readonly ActorMailPool _actorMailPool = new();
        private readonly SynchronizedArrayQueue<IActorSchedulable> _scheduledActors = [];
        private readonly ActorYieldTaskPool _yieldTaskPool = new();

        private volatile bool _isStopped;
        private readonly DispatcherSynchronizationContext _syncContext;

        public ActorDispatcher(int dispatcherId)
        {
            _dispatcherId = dispatcherId;
            _syncContext = new DispatcherSynchronizationContext(this);
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
            SynchronizationContext.SetSynchronizationContext(_syncContext);
            SpinWait spinWait = new();

            while (!_isStopped)
            {
                while (_scheduledActors.TryRead(out IActorSchedulable actorSchedulable))
                {
                    actorSchedulable.Execute();
                    spinWait.Reset();
                }

                if (_isStopped)
                {
                    break;
                }

                spinWait.SpinOnce();
                if (spinWait.Count < 30)
                {
                    continue;
                }

                _signal.Wait();
                _signal.Reset();
            }
            CurrentActorDispatcher = null;
            SynchronizationContext.SetSynchronizationContext(null);
        }
        public void Dispose()
        {
            _isStopped = true;
            _signal.Set();
            if (Thread.CurrentThread != _workerThread)
            {
                _workerThread.Join();
            }
            _signal.Dispose();
        }
        internal void Schedule(IActorSchedulable actor)
        {
            _scheduledActors.Add(actor);
            _signal.Set();
        }
        internal void EnqueueContinuation(SendOrPostCallback sendOrPostCallback, object state)
        {
            var yieldTask = _yieldTaskPool.Pop();
            yieldTask.Set(sendOrPostCallback, state);
            Schedule(yieldTask);
        }
    }
}
