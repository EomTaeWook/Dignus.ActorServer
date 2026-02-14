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
    public sealed class TransportActor(ISession session) : ActorBase
    {
        private ISession _session = session;
        protected override ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            var session = _session;

            if (session == null)
            {
                return ValueTask.CompletedTask;
            }

            if (message is BinaryMessage binaryMessage)
            {
                session.Send(binaryMessage.Data);
            }
            return ValueTask.CompletedTask;
        }
        internal override void Cleanup()
        {
            base.Cleanup();
        }
        public override void OnKill()
        {
            var session = _session;
            if (session != null)
            {
                session.Dispose();
                _session = null;
            }
        }
    }
}
