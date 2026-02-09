// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Serializer;
using Dignus.Sockets.Interfaces;

namespace Dignus.Actor.Network.Actors
{
    public class NetworkActor(ISession session, IMessageSerializer serializer) : IActorRef
    {
        public void Dispose()
        {
            session.Dispose();
        }

        public void Post(IActorMessage message, IActorRef sender = null)
        {
            session.Send(serializer.MakeSendBuffer(message));
        }
    }
}
