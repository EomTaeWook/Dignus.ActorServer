// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Messages;
using Dignus.Sockets.Interfaces;
using System.Threading.Tasks;

namespace Dignus.Actor.Network.Actors
{
    public class TransportActor(ISession session) : ActorBase
    {
        private ISession _session = session;

        public void Dispose()
        {
            var session = _session;

            if (session == null)
            {
                return;
            }
            _session = null;
            session.Dispose();
        }

        protected override Task OnReceive(IActorMessage message, IActorRef sender = null)
        {
            var session = _session;

            if (session == null)
            {
                return Task.CompletedTask;
            }

            if (message is RawMessage rawMessage)
            {
                session.Send(rawMessage.Data);
            }
            return Task.CompletedTask;
        }
    }
}
