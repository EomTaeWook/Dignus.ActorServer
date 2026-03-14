// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Codec;
using Dignus.Sockets;
using System;

namespace Dignus.Actor.Network.Options
{
    public static class OptionsBuilderExtensions
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
        public static TBuilder UseDecoder<TBuilder>(this TBuilder builder, IActorMessageDecoder decoder)
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
    }
}
