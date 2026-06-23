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

        private long _nextRequestId;

        private readonly ConcurrentDictionary<long, IAskTimeout> _askReplyActorRefByRequestId = new ConcurrentDictionary<long, IAskTimeout>();
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

                if (_askReplyActorRefByRequestId.IsEmpty)
                {
                    Interlocked.Exchange(ref _isSweepSignaled, 0);

                    if (_askReplyActorRefByRequestId.IsEmpty)
                    {
                        _sweepSignal.WaitOne();
                    }

                    continue;
                }

                long currentUtcTicks = DateTime.UtcNow.Ticks;

                foreach (var askAwaiterByRequestId in _askReplyActorRefByRequestId)
                {
                    if (askAwaiterByRequestId.Value.DeadlineAtTicks > currentUtcTicks)
                    {
                        continue;
                    }

                    OnTimeout(askAwaiterByRequestId.Key);
                }

                Thread.Sleep(TimeoutSweepIntervalMilliseconds);
            }
        }

        internal bool TryRemove(long requestId)
        {
            return _askReplyActorRefByRequestId.TryRemove(requestId, out _);
        }

        private void OnTimeout(long requestId)
        {
            if (_askReplyActorRefByRequestId.TryRemove(requestId, out var askAwaiter) == false)
            {
                return;
            }
            askAwaiter.SetTimeout();
        }

        internal AskReplyActorRef<TResponse> Register<TResponse>(TimeSpan timeout) where TResponse : IActorMessage
        {
            long requestId = Interlocked.Increment(ref _nextRequestId);
            var askAwaiter = new AskReplyActorRef<TResponse>(requestId, this, timeout);

            if (_askReplyActorRefByRequestId.TryAdd(requestId, askAwaiter) == false)
            {
                throw new InvalidOperationException($"failed to register ask request. requestId:{requestId}");
            }

            if (Interlocked.Exchange(ref _isSweepSignaled, 1) == 0)
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

            foreach (var askAwaiterByRequestId in _askReplyActorRefByRequestId)
            {
                OnTimeout(askAwaiterByRequestId.Key);
            }
            _sweepSignal.Dispose();
        }
    }
}