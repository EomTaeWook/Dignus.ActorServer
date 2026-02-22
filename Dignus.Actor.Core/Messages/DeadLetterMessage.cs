// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using System;

namespace Dignus.Actor.Core.Messages
{
    public enum DeadLetterReason
    {
        Unknown = 0,
        MailboxFull,
        RecipientInvalidated,
        ActorStopped,
        ActorSystemDisposed,
    }
    public class DeadLetterMessage(
        IActorMessage originalMessage,
        IActorRef sender,
        int recipientActorId,
        DeadLetterReason reason) : IActorMessage
    {
        public IActorMessage OriginalMessage { get; } = originalMessage;
        public IActorRef Sender { get; } = sender;
        public int RecipientActorId { get; } = recipientActorId;
        public DateTime DetectedTimestamp { get; } = DateTime.UtcNow;

        public DeadLetterReason Reason { get; } = reason;
    }
}
