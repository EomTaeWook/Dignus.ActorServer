// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Sockets;
namespace Dignus.Actor.Network
{
    public class ActorSessionConfiguration
    {
        public SocketOption SocketOption { get; private set; }

        public ActorSessionConfiguration(ActorSessionSetupFactoryDelegate sessionSetupFactory,
            SocketOption socketOption = null)
        {

        }
    }
}
