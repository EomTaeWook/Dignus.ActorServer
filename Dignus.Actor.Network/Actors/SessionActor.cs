// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network.Messages;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets.Interfaces;

namespace Dignus.Actor.Network.Actors
{
    public abstract class SessionActor(IActorRef transportRef
        ) : ActorBase
    {
        protected IActorRef TransportRef => transportRef;

        private IActorMessageSerializer _messageSerializer;

        internal void Initialize(IActorMessageSerializer serializer)
        {
            _messageSerializer = serializer;
        }

        public void Post(byte[] bytes)
        {
            Post(transportRef, new BinaryMessage(bytes));
        }
        public void Post(IPacket packet)
        {
            var buffer = _messageSerializer.MakeSendBuffer(packet);
            Post(transportRef, new BinaryMessage(buffer));
        }
        public void Post(INetworkActorMessage message)
        {
            var buffer = _messageSerializer.MakeSendBuffer(message);
            Post(transportRef, new BinaryMessage(buffer));
        }

        internal override void Cleanup()
        {
            transportRef.Kill();
            base.Cleanup();
        }
    }
}
