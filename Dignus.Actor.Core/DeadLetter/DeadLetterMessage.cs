// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Abstractions;
using System;

namespace Dignus.Actor.Core.DeadLetter
{
    public class DeadLetterMessage(
        IActorMessage message,
        IActorRef sender,
        int recipientActorId,
        DeadLetterReason reason) : IActorMessage
    {
        public IActorMessage Message { get; } = message;
        public IActorRef Sender { get; } = sender;
        public int RecipientActorId { get; } = recipientActorId;
        public DateTime DetectedTimestamp { get; } = DateTime.UtcNow;
        public DeadLetterReason Reason { get; } = reason;
    }
}
