// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.ObjectPools;
using System;

namespace Dignus.Actor.Core.Messages
{
    internal class ActorMail
    {
        public IActorMessage Message { get; set; }
        public IActorRef Sender { get; set; }

        private readonly ActorMailPool _pool;

        internal ActorMail(ActorMailPool pool)
        {
            _pool = pool;
        }
        public ActorMail()
        {

        }
        public void Recycle()
        {
            if(_pool == null)
            {
                throw new InvalidOperationException("ActorMail has no pool.");
            }
            _pool.Push(this);
        }
    }
}
