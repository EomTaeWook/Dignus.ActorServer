// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Messages;
using System;

namespace Dignus.Actor.Core.DeadLetter
{
    public record ActorExceptionMessage(Exception Exception) : IActorMessage
    {
    }
}
