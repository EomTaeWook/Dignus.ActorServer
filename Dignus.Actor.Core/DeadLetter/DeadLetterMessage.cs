// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Abstractions;
using System;

namespace Dignus.Actor.Core.DeadLetter
{
    public class DeadLetterMessage: IActorMessage
    {
        public IActorMessage Message { get; }
        public IActorRef Sender { get; }
        public long RecipientActorId { get; }
        public DateTime DetectedTimestamp { get; } = DateTime.UtcNow;
        public DeadLetterReason Reason { get; }

        public DeadLetterMessage(IActorMessage message,
            IActorRef sender,
            long recipientActorId,
            DeadLetterReason reason)
        {
            Message = message;
            Sender = sender;
            RecipientActorId = recipientActorId;
            Reason = reason;
        }
    }
}
