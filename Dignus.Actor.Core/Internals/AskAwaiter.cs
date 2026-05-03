// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Internals
{
    internal interface IAskAwaiter : IDisposable
    {
        void SetResponse(IActorMessage responseMessage);
        void SetTimeout();
    }

    internal sealed class AskAwaiter<TResponse> : IAskAwaiter
        where TResponse : IActorMessage
    {
        public Task<TResponse> Task => _taskCompletionSource.Task;

        private readonly long _requestId;
        private readonly AskSystem _askSystem;
        private readonly TaskCompletionSource<TResponse> _taskCompletionSource;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public AskAwaiter(long requestId, TimeSpan timeout, AskSystem askSystem)
        {
            _requestId = requestId;
            _askSystem = askSystem;
            _taskCompletionSource = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

            _cancellationTokenSource = new CancellationTokenSource(timeout);
            _cancellationTokenSource.Token.Register(OnTimeout);
        }

        private void OnTimeout()
        {
            _askSystem.OnTimeout(_requestId);
        }

        public void SetResponse(IActorMessage responseMessage)
        {
            if (responseMessage is TResponse response)
            {
                _taskCompletionSource.TrySetResult(response);
                return;
            }

            _taskCompletionSource.TrySetException(new InvalidOperationException($"Ask response type mismatch. expected:{typeof(TResponse).Name}, actual:{responseMessage.GetType().Name}"));
        }

        public void SetTimeout()
        {
            _taskCompletionSource.TrySetException(new TimeoutException());
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}