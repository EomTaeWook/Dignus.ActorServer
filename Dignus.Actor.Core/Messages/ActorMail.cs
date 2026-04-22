// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Abstractions;

namespace Dignus.Actor.Core.Messages
{
    public readonly struct ActorMail
    {
        public readonly IActorMessage Message { get; }
        public readonly IActorRef Sender { get;}
        public ActorMail(IActorMessage message, IActorRef sender)
        {
            Message = message;
            Sender = sender;
        }
    }
}
