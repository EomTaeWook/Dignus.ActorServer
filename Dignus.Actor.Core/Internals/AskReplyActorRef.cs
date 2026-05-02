// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Abstractions;
using Dignus.Actor.Core.Messages;
using System;

namespace Dignus.Actor.Core.Internals
{
    internal class AskReplyActorRef : IActorRef
    {
        private readonly long _requestId;
        private readonly AskSystem _askSystem;

        public AskReplyActorRef(long requestId, AskSystem askSystem)
        {
            _requestId = requestId;
            _askSystem = askSystem;
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
