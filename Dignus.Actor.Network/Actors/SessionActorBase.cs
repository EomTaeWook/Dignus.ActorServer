// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;

namespace Dignus.Actor.Network.Actors
{
    public abstract class SessionActorBase : ActorBase
    {
        private INetworkSessionRef _networkSessionRef;
        protected INetworkSession NetworkSession => _networkSessionRef;

        internal void SetNetworkSessionRef(INetworkSessionRef networkSessionRef)
        {
            _networkSessionRef = networkSessionRef;
        }
        internal override void Cleanup()
        {
            _networkSessionRef.CloseSession();
            base.Cleanup();
        }
    }
}
