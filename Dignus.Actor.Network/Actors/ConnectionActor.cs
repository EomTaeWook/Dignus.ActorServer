// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Sockets.Interfaces;
using System.Threading.Tasks;

namespace Dignus.Actor.Network.Actors
{
    public class ConnectionActor(ISession session) : ActorBase
    {
        public void Dispose()
        {
            session.Dispose();
        }

        protected override Task OnReceive(IActorMessage message, IActorRef sender = null)
        {
            return Task.CompletedTask;
        }
    }
}
