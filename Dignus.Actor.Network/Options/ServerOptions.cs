// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

namespace Dignus.Actor.Network.Options
{
    public class ServerOptions : IActorServerOptions
    {
        public ActorSystemOptions ActorSystem { get; set; } = new ActorSystemOptions();
        public ActorNetworkOptions Network { get; set; } = new ActorNetworkOptions();

        public static ServerOptionsBuilder Builder() => new();
        public sealed class ServerOptionsBuilder : IActorOptionsBuilderBase<ServerOptions>
        {
            private readonly ServerOptions _options = new()
            {
                ActorSystem = new ActorSystemOptions(),
                Network = new ActorNetworkOptions(),
            };

            public ServerOptions Options => _options;

            public ServerOptions Build()
            {
                return _options;
            }
        }
    }
}
