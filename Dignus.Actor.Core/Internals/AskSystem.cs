// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Dignus.Actor.Core.Internals
{
    internal class AskSystem
    {
        private const int TimeoutSweepIntervalMilliseconds = 100;

        private readonly ConcurrentDictionary<IAskTimeout, byte> _askReplyActorRefs = new ConcurrentDictionary<IAskTimeout, byte>();
        private readonly Thread _timeoutSweepThread;
        private readonly AutoResetEvent _sweepSignal = new AutoResetEvent(false);

        private int _isSweepSignaled;
        private int _isDisposed;

        public AskSystem()
        {
            _timeoutSweepThread = new Thread(ProcessTimeouts)
            {
                IsBackground = true,
                Name = "AskTimeoutSweep"
            };
            _timeoutSweepThread.Start();
        }

        private void ProcessTimeouts()
        {
            while (true)
            {
                if (_isDisposed == 1)
                {
                    break;
                }

                if (_askReplyActorRefs.IsEmpty)
                {
                    Interlocked.Exchange(ref _isSweepSignaled, 0);

                    if (_askReplyActorRefs.IsEmpty)
                    {
                        _sweepSignal.WaitOne();
                        continue;
                    }

                    Interlocked.CompareExchange(ref _isSweepSignaled, 1, 0);
                    continue;
                }

                long now = DateTime.UtcNow.Ticks;

                foreach (var item in _askReplyActorRefs)
                {
                    if (item.Key.DeadlineAtTicks > now)
                    {
                        continue;
                    }

                    OnTimeout(item.Key);
                }

                _sweepSignal.WaitOne(TimeoutSweepIntervalMilliseconds);
            }
        }

        internal bool TryRemove(IAskTimeout askTimeout)
        {
            return _askReplyActorRefs.TryRemove(askTimeout, out _);
        }

        private void OnTimeout(IAskTimeout askTimeout)
        {
            if (TryRemove(askTimeout) == false)
            {
                return;
            }

            askTimeout.SetTimeout();
        }

        internal AskReplyActorRef<TResponse> Register<TResponse>(TimeSpan timeout) where TResponse : IActorMessage
        {
            var askAwaiter = new AskReplyActorRef<TResponse>(this, timeout);

            if (_askReplyActorRefs.TryAdd(askAwaiter, 0) == false)
            {
                throw new InvalidOperationException("failed to register ask request.");
            }

            if(_isSweepSignaled == 1)
            {
                return askAwaiter;
            }

            if (Interlocked.CompareExchange(ref _isSweepSignaled, 1, 0) == 0)
            {
                _sweepSignal.Set();
            }

            return askAwaiter;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            {
                return;
            }

            _sweepSignal.Set();

            if (Thread.CurrentThread != _timeoutSweepThread)
            {
                _timeoutSweepThread.Join();
            }

            foreach (var askAwaiter in _askReplyActorRefs.Keys)
            {
                OnTimeout(askAwaiter);
            }

            _sweepSignal.Dispose();
        }
    }
}