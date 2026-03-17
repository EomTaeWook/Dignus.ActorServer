// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

namespace Dignus.Actor.Core.DeadLetter
{
    public enum DeadLetterReason
    {
        Unknown = 0,
        MailboxFull,
        RecipientInvalidated,
        ActorStopped,
        ActorSystemDisposed,
        ExecutionException
    }
}
