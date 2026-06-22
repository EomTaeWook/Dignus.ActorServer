// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Internals
{
    internal class AskSystem
    {
        private const int TimeoutSweepIntervalMilliseconds = 10;

        private long _nextRequestId;

        private readonly ConcurrentDictionary<long, IAskAwaiter> _askAwaiterByRequestId = new ConcurrentDictionary<long, IAskAwaiter>();

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
                if(_isDisposed == 1)
                {
                    break;
                }
                if (_askAwaiterByRequestId.IsEmpty)
                {
                    Interlocked.Exchange(ref _isSweepSignaled, 0);

                    if (_askAwaiterByRequestId.IsEmpty)
                    {
                        _sweepSignal.WaitOne();
                    }
                    continue;
                }

                long currentUtcTicks = DateTime.UtcNow.Ticks;

                foreach (var askAwaiterByRequestId in _askAwaiterByRequestId)
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

        private void OnTimeout(long requestId)
        {
            if (_askAwaiterByRequestId.TryRemove(requestId, out var askAwaiter) == false)
            {
                return;
            }
            askAwaiter.SetTimeout();
            askAwaiter.Dispose();
        }

        public long Register<TResponse>(TimeSpan timeout, out ValueTask<TResponse> responseTask) where TResponse : IActorMessage
        {
            long requestId = Interlocked.Increment(ref _nextRequestId);

            var askAwaiter = new AskAwaiter<TResponse>(timeout);

            if (_askAwaiterByRequestId.TryAdd(requestId, askAwaiter) == false)
            {
                askAwaiter.Dispose();
                throw new InvalidOperationException($"failed to register ask request. requestId:{requestId}");
            }

            responseTask = askAwaiter.ValueTask;

            if (Interlocked.Exchange(ref _isSweepSignaled, 1) == 0)
            {
                _sweepSignal.Set();
            }
            return requestId;
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

            foreach (var askAwaiterByRequestId in _askAwaiterByRequestId)
            {
                OnTimeout(askAwaiterByRequestId.Key);
            }

            _sweepSignal.Dispose();
        }

        public bool TrySetResponse(long requestId, IActorMessage responseMessage)
        {
            if (_askAwaiterByRequestId.TryRemove(requestId, out var askAwaiter) == false)
            {
                return false;
            }

            askAwaiter.SetResponse(responseMessage);
            askAwaiter.Dispose();
            return true;
        }

    }
}
