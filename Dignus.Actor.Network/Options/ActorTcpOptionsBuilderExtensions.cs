// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using System;

namespace Dignus.Actor.Network.Options
{
    public static class ActorTcpOptionsBuilderExtensions
    {
        public static TBuilder UseInitialSessionPoolSize<TBuilder>(this TBuilder builder,
            int initialSessionPoolSize)
            where TBuilder : IActorOptionsBuilderBase<ServerOptions>
        {
            if (initialSessionPoolSize < 0)
            {
                throw new ArgumentOutOfRangeException("Initial session pool size must be zero or greater.");
            }

            builder.Options.InitialSessionPoolSize = initialSessionPoolSize;
            return builder;
        }
    }
}
