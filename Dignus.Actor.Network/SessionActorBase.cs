// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core;
using Dignus.Actor.Network.Internals;
using System;

namespace Dignus.Actor.Network
{
    public abstract class SessionActorBase: ActorBase
    {
        private NetworkSessionRef _networkSessionRef;
        internal protected INetworkSessionRef NetworkSession => _networkSessionRef;

        internal void Initialize(NetworkSessionRef networkSessionRef)
        {
            ArgumentNullException.ThrowIfNull(networkSessionRef);
            _networkSessionRef = networkSessionRef;
        }
        internal override void KillInternal()
        {
            if(_networkSessionRef != null)
            {
                _networkSessionRef.Close();
            }
            base.KillInternal();
        }
    }
}
