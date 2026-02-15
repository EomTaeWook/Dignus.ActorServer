// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network.Messages;

namespace Dignus.Actor.Network.Actors
{
    public abstract class SessionActor(IActorRef transportRef
        ) : ActorBase
    {
        protected IActorRef TransportRef => transportRef;

        public void Post(byte[] bytes)
        {
            Post(transportRef, new BinaryMessage(bytes));
        }

        internal override void Cleanup()
        {
            transportRef.Kill();
            base.Cleanup();
        }
    }
}
