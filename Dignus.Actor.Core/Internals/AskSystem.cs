// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Internals
{
    internal class AskSystem
    {
        private long _requestId;
        private readonly ConcurrentDictionary<long, IAskAwaiter> _askAwaiterByRequestId = new ConcurrentDictionary<long, IAskAwaiter>();
        internal void OnTimeout(long requestId)
        {
            if (_askAwaiterByRequestId.TryRemove(requestId, out var askAwaiter) == false)
            {
                return;
            }
            askAwaiter.SetTimeout();
            askAwaiter.Dispose();
        }

        public long Register<TResponse>(TimeSpan timeout, out Task<TResponse> responseTask)
            where TResponse : IActorMessage
        {
            long requestId = Interlocked.Increment(ref _requestId);

            var askAwaiter = new AskAwaiter<TResponse>(requestId, timeout, this);

            if (_askAwaiterByRequestId.TryAdd(requestId, askAwaiter) == false)
            {
                askAwaiter.Dispose();
                throw new InvalidOperationException($"failed to register ask request. requestId:{requestId}");
            }

            responseTask = askAwaiter.Task;
            return requestId;
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
