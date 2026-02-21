// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Messages;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;

namespace Dignus.Actor.Network
{
    public interface INetworkSession
    {
        SendResult Send(byte[] bytes);
        SendResult Send(IPacket packet);
        SendResult Send(INetworkActorMessage message);

        SendResult SendAsync(byte[] bytes);
        SendResult SendAsync(IPacket packet);
        SendResult SendAsync(INetworkActorMessage message);
    }
}
