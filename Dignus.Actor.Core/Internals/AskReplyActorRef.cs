// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Abstractions;
using Dignus.Actor.Core.Messages;
using System;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Internals
{
    internal class AskReplyActorRef<TResponse> : IActorRef where TResponse : IActorMessage
    {
        private readonly long _requestId;
        private readonly AskSystem _askSystem;

        public ValueTask<TResponse> ResponseTask { get; }

        public AskReplyActorRef(TimeSpan timeout, AskSystem askSystem)
        {
            _askSystem = askSystem;
            _requestId = askSystem.Register(timeout, out ValueTask<TResponse> responseTask);

            ResponseTask = responseTask;
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
            _askSystem.TrySetResponse(_requestId, message);
        }

        public void Post(in ActorMail actorMail)
        {
            Post(actorMail.Message, actorMail.Sender);
        }
    }
}
