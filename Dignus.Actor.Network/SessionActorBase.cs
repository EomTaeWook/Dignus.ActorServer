// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core;
using System;

namespace Dignus.Actor.Network
{
    public abstract class SessionActorBase : ActorBase
    {
        internal int? DispatcherIndex => _dispatcherIndex;

        private INetworkSessionRef _networkSessionRef;

        protected INetworkSession NetworkSession => _networkSessionRef;

        private readonly int? _dispatcherIndex = -1;
        public SessionActorBase() : this(-1)
        {
        }
        public SessionActorBase(int? dispatcherIndex)
        {
            _dispatcherIndex = dispatcherIndex;
        }

        internal void Initialize(INetworkSessionRef networkSessionRef)
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
