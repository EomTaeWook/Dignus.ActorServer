// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Abstractions;
using Dignus.Actor.Core.Messages;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Dignus.Actor.Core.Internals
{
    internal sealed class AskReplyActorRef<TResponse> : IActorRef, IAskTimeout, IValueTaskSource<TResponse> 
        where TResponse : IActorMessage
    {
        public ValueTask<TResponse> ValueTask => new ValueTask<TResponse>(this, _valueTaskSourceCore.Version);
        public long DeadlineAtTicks => _deadlineAtTicks;

        private readonly long _deadlineAtTicks;
        private readonly long _requestId;
        private readonly AskSystem _askSystem;

        private ManualResetValueTaskSourceCore<TResponse> _valueTaskSourceCore;

        public AskReplyActorRef(long requestId, AskSystem askSystem, TimeSpan timeout)
        {
            _requestId = requestId;
            _askSystem = askSystem;
            _deadlineAtTicks = DateTime.UtcNow.Add(timeout).Ticks;
            _valueTaskSourceCore = new ManualResetValueTaskSourceCore<TResponse>();
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

            SetResponse(message);
        }
        public TResponse GetResult(short token)
        {
            return _valueTaskSourceCore.GetResult(token);
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            return _valueTaskSourceCore.GetStatus(token);
        }

        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _valueTaskSourceCore.OnCompleted(continuation, state, token, flags);
        }
        public void SetTimeout()
        {
            _valueTaskSourceCore.SetException(new TimeoutException());
        }
        private void SetResponse(IActorMessage responseMessage)
        {
            if (_askSystem.TryRemove(_requestId) == false)
            {
                return;
            }

            if (responseMessage is TResponse response)
            {
                _valueTaskSourceCore.SetResult(response);
                return;
            }

            _valueTaskSourceCore.SetException(new InvalidOperationException($"Ask response type mismatch. expected:{typeof(TResponse).Name}, actual:{responseMessage.GetType().Name}"));
        }
        public void Post(in ActorMail actorMail)
        {
            Post(actorMail.Message, actorMail.Sender);
        }
    }
}
