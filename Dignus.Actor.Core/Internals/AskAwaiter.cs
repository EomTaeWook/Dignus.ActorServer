// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Abstractions;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Dignus.Actor.Core.Internals
{
    internal interface IAskAwaiter : IDisposable
    {
        long DeadlineAtTicks { get; }
        void SetResponse(IActorMessage responseMessage);
        void SetTimeout();
    }

    internal sealed class AskAwaiter<TResponse> : IAskAwaiter, IValueTaskSource<TResponse>
        where TResponse : IActorMessage
    {
        public ValueTask<TResponse> ValueTask => new ValueTask<TResponse>(this, _valueTaskSource.Version);

        public long DeadlineAtTicks => _deadlineAtTicks;

        private readonly long _deadlineAtTicks = 0;
        private ManualResetValueTaskSourceCore<TResponse> _valueTaskSource;
        public AskAwaiter(TimeSpan timeout)
        {
            _deadlineAtTicks = DateTime.UtcNow.Add(timeout).Ticks;
            _valueTaskSource.RunContinuationsAsynchronously = true;
        }
        public void SetResponse(IActorMessage responseMessage)
        {
            if (responseMessage is TResponse response)
            {
                _valueTaskSource.SetResult(response);
                return;
            }

            _valueTaskSource.SetException(new InvalidOperationException($"Ask response type mismatch. expected:{typeof(TResponse).Name}, actual:{responseMessage.GetType().Name}"));
        }

        public void SetTimeout()
        {
            _valueTaskSource.SetException(new TimeoutException());
        }
        public void Dispose()
        {
        }

        public TResponse GetResult(short token)
        {
            return _valueTaskSource.GetResult(token);
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            return _valueTaskSource.GetStatus(token);
        }

        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _valueTaskSource.OnCompleted(continuation, state, token, flags);
        }
    }
}