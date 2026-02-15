// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;

namespace Dignus.Actor.Core.Messages
{
    public readonly struct ActorMail(IActorMessage Message, IActorRef Sender)
    {
        public readonly IActorMessage Message = Message;
        public readonly IActorRef Sender = Sender;
    }
}
