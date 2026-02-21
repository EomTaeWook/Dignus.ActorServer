// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network.Messages;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using System;

namespace Dignus.Actor.Network.Actors
{
    internal class NetworkSessionRef : INetworkSessionRef
    {
        private readonly IActorRef _sessionActorRef;
        private readonly ISession _session;
        private readonly IActorMessageSerializer _serializer;

        public NetworkSessionRef(IActorRef sessionActorRef,
            ISession session,
            IActorMessageSerializer serializer)
        {
            ArgumentNullException.ThrowIfNull(sessionActorRef);
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(serializer);

            _sessionActorRef = sessionActorRef;
            _session = session;
            _serializer = serializer;
        }
        public void Kill()
        {
            _sessionActorRef.Kill();
        }

        public void Close()
        {
            _session.Dispose();
        }

        public void Post(IActorMessage message, IActorRef sender = null)
        {
            _sessionActorRef.Post(message, sender);
        }

        public void Post(in ActorMail actorMail)
        {
            _sessionActorRef.Post(actorMail);
        }

        public SendResult Send(byte[] bytes)
        {
            return _session.Send(bytes);
        }

        public SendResult Send(IPacket packet)
        {
            return _session.Send(_serializer.MakeSendBuffer(packet));
        }

        public SendResult Send(INetworkActorMessage message)
        {
            return _session.Send(_serializer.MakeSendBuffer(message));
        }
        public SendResult SendAsync(byte[] bytes)
        {
            return _session.SendAsync(bytes);
        }
        public SendResult SendAsync(IPacket packet)
        {
            return _session.SendAsync(_serializer.MakeSendBuffer(packet));
        }

        public SendResult SendAsync(INetworkActorMessage message)
        {
            return _session.SendAsync(_serializer.MakeSendBuffer(message));
        }
    }
}
