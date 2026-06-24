// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Abstractions;
using Dignus.Actor.Core.Messages;
using System;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Internals
{
    internal sealed class AskReplyActorRef<TResponse> : IActorRef, IAskTimeout
        where TResponse : IActorMessage
    {
        public ValueTask<TResponse> ValueTask => new ValueTask<TResponse>(_taskCompletionSource.Task);
        public long DeadlineAtTicks => _deadlineAtTicks;

        private readonly long _deadlineAtTicks;
        private readonly long _requestId;
        private readonly AskSystem _askSystem;
        private readonly TaskCompletionSource<TResponse> _taskCompletionSource;

        public AskReplyActorRef(long requestId, AskSystem askSystem, TimeSpan timeout)
        {
            _requestId = requestId;
            _askSystem = askSystem;
            _deadlineAtTicks = DateTime.UtcNow.Add(timeout).Ticks;
            _taskCompletionSource = new TaskCompletionSource<TResponse>();
        }

        public void Kill()
        {
        }

        public void Post(IActorMessage message, IActorRef sender = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (_askSystem.TryRemove(_requestId) == false)
            {
                return;
            }

            if (message is TResponse response)
            {
                _taskCompletionSource.TrySetResult(response);
                return;
            }

            _taskCompletionSource.TrySetException(new InvalidOperationException($"Ask response type mismatch. expected:{typeof(TResponse).Name}, actual:{message.GetType().Name}"));
        }

        public void SetTimeout()
        {
            _taskCompletionSource.TrySetException(new TimeoutException());
        }
        public void Post(in ActorMail actorMail)
        {
            Post(actorMail.Message, actorMail.Sender);
        }
    }
}
