// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Abstractions;
using System;

namespace Dignus.Actor.Core.DeadLetter
{
    public class ActorExceptionMessage : IActorMessage
    {
        public Exception Exception { get; }

        public ActorExceptionMessage(Exception exception)
        {
            Exception = exception;
        }
    }
}
