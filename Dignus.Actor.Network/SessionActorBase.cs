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
        internal NetworkSessionRef NetworkSessionRef { get; private set; }
        protected INetworkSession NetworkSession => NetworkSessionRef;

        internal void Initialize(NetworkSessionRef networkSessionRef)
        {
            ArgumentNullException.ThrowIfNull(networkSessionRef);
            NetworkSessionRef = networkSessionRef;
        }
        internal override void KillInternal()
        {
            if(NetworkSessionRef != null)
            {
                NetworkSessionRef.Close();
            }
            base.KillInternal();
        }
    }
}
