// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

namespace Dignus.Actor.Core.Internals
{
    internal enum EnqueueResult
    {
        Success,
        ActorStopped,
        MailboxFull
    }
}
