// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

namespace Dignus.Actor.Network.Options
{
    public sealed class TlsServerOptions : IActorTlsServerOptions
    {
        public ActorSystemOptions ActorSystem { get; set; }
        public ActorNetworkOptions Network { get; set; }
        public Sockets.Tls.TlsServerOptions TlsOptions { get; set; }

        public static TlsServerOptionsBuilder Builder() => new();

        public sealed class TlsServerOptionsBuilder : IActorOptionsBuilderBase<TlsServerOptions>
        {
            private readonly TlsServerOptions _options = new()
            {
                ActorSystem = new ActorSystemOptions(),
                Network = new ActorNetworkOptions(),
            };

            public TlsServerOptions Options => _options;

            public TlsServerOptions Build()
            {
                return _options;
            }
        }

    }
}
