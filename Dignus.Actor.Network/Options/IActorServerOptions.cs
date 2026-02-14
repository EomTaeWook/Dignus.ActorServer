// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Sockets.Tls;

namespace Dignus.Actor.Network.Options
{
    public interface IActorServerOptions
    {
        ActorSystemOptions ActorSystem { get; }
        ActorNetworkOptions Network { get; }
    }
    public interface IActorOptionsBuilderBase<out TOptions> where TOptions : IActorServerOptions
    {
        TOptions Options { get; }
        TOptions Build();
    }

    public interface IActorTlsServerOptions : IActorServerOptions
    {
        Sockets.Tls.TlsServerOptions TlsOptions { get; }
    }
}
