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
        public ActorMailPool MailPool => _actorMailPool;

        private readonly int _dispatcherId;

        private readonly SynchronizedArrayQueue<IActorSchedulable> _scheduledActors = new();

        private readonly ActorMailPool _actorMailPool = new();
        private readonly ActorYieldTaskPool _yieldTaskPool = new();
        private volatile bool _isStopped;
        private readonly DispatcherSynchronizationContext _synchronizationContext;

        private int _isProcessing = 0;

        private Thread _workerThread;
        private ManualResetEventSlim _signal = new ManualResetEventSlim(false);

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
            _isProcessing = 1;
        }

        private void ProcessScheduledActors()
        {
            CurrentActorDispatcher = this;
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

            while (!_isStopped)
            {
                while (_scheduledActors.TryRead(out IActorSchedulable actorSchedulable))
                {
                    Console.WriteLine($"Processing scheduledActors : {actorSchedulable}");
                    actorSchedulable.Execute();

                    if (_isStopped)
                    {
                        return;
                    }
                }

                if (_scheduledActors.Count > 0)
                {
                    Console.WriteLine($"scheduledActors count : {_scheduledActors.Count}");
                    continue;
                }
                _signal.Reset();
                Console.WriteLine($"scheduledActors count : {_scheduledActors.Count}");
                if (_scheduledActors.Count == 0)
                {
                    if (Interlocked.CompareExchange(ref _isProcessing, 0, 1) == 1)
                    {
                        _signal.Wait();
                    }
                    else
                    {
                        Console.WriteLine("!!!");
                    }
                }
            }
        }

        public void Dispose()
        {
            _isStopped = true;
        }

        internal void Schedule(IActorSchedulable actorSchedulable)
        {
            _scheduledActors.Add(actorSchedulable);

            if(_isProcessing == 1)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) == 0)
            {
                _signal.Set();
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

