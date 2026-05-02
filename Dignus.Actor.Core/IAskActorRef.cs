// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Abstractions;
using System.Threading.Tasks;

namespace Dignus.Actor.Core
{
    public interface IAskActorRef : IActorRef
    {
        Task<TResponse> AskAsync<TResponse>(IActorMessage message, int timeoutMilliseconds) where TResponse : IActorMessage;
    }
}
