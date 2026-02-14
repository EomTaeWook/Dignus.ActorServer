// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Protocol;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets;
using System;

namespace Dignus.Actor.Network.Options
{
    public static class ActorOptionsBuilderExtensions
    {
        public static TBuilder WithDispatcherThreads<TBuilder>(this TBuilder builder, int count)
            where TBuilder : IActorOptionsBuilderBase<IActorServerOptions>
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "DispatcherThreadCount must be > 0");
            }

            builder.Options.ActorSystem.DispatcherThreadCount = count;
            return builder;
        }
        public static TBuilder UseSerializer<TBuilder>(this TBuilder builder, IActorMessageSerializer serializer)
            where TBuilder : IActorOptionsBuilderBase<IActorServerOptions>
        {
            ArgumentNullException.ThrowIfNull(serializer);
            builder.Options.Network.MessageSerializer = serializer;
            return builder;
        }
        public static TBuilder UseDecoder<TBuilder>(this TBuilder builder, IMessageDecoder decoder)
            where TBuilder : IActorOptionsBuilderBase<IActorServerOptions>
        {
            ArgumentNullException.ThrowIfNull(decoder);
            builder.Options.Network.Decoder = decoder;
            return builder;
        }
        public static TBuilder UseSocketOption<TBuilder>(this TBuilder builder, SocketOption socketOption)
            where TBuilder : IActorOptionsBuilderBase<IActorServerOptions>
        {
            ArgumentNullException.ThrowIfNull(socketOption);
            builder.Options.Network.SocketOption = socketOption;
            return builder;
        }
        public static TBuilder WithInitialPoolSize<TBuilder>(this TBuilder builder, int size)
            where TBuilder : IActorOptionsBuilderBase<IActorServerOptions>
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "InitialSessionPoolSize must be >= 0");
            }

            builder.Options.Network.InitialSessionPoolSize = size;
            return builder;
        }
    }
}
