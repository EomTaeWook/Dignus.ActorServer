// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;

namespace Dignus.Actor.Network.Actors
{
    public abstract class SessionActorBase : ActorBase
    {
        private INetworkSession _networkSession;
        protected INetworkSession NetworkSession => _networkSession;

        internal void SetNetworkSession(INetworkSession networkSession)
        {
            _networkSession = networkSession;
        }
        internal override void Cleanup()
        {
            base.Cleanup();
        }
    }
}
