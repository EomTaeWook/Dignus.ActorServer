// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Network.Protocol;
using Dignus.Actor.Network.Serialization;
using Dignus.Sockets;
using Dignus.Sockets.Tls;
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Dignus.Actor.Network
{
    public sealed class ActorSystemOptions
    {
        public int DispatcherThreadCount { get; set; } = Environment.ProcessorCount;
    }
    public sealed class ActorNetworkOptions
    {
        public IActorMessageSerializer MessageSerializer { get; set; }
        public IMessageDecoder Decoder { get; set; }
        public SocketOption SocketOption { get; set; }
        public int InitialSessionPoolSize { get; set; } = 0;
    }

    public sealed class ActorTlsServerOptions
    {
        public ActorSystemOptions ActorSystem { get; set; }
        public ActorNetworkOptions Network { get; set; }
        public TlsServerOptions TlsOptions { get; set; }

        public static ActorTlsServerOptionsBuilder Builder() => new();

        public sealed class ActorTlsServerOptionsBuilder 
        {
            private readonly ActorTlsServerOptions _options = new()
            {
                ActorSystem = new ActorSystemOptions(),
                Network = new ActorNetworkOptions()
            };
            public ActorTlsServerOptionsBuilder WithDispatcherThreads(int count)
            {
                if (count <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(count), "DispatcherThreadCount must be > 0");
                }
                _options.ActorSystem.DispatcherThreadCount = count;
                return this;
            }
            public ActorTlsServerOptionsBuilder UseSerializer(IActorMessageSerializer serializer)
            {
                ArgumentNullException.ThrowIfNull(serializer);
                _options.Network.MessageSerializer = serializer;
                return this;
            }
            public ActorTlsServerOptionsBuilder UseDecoder(IMessageDecoder decoder)
            {
                ArgumentNullException.ThrowIfNull(decoder);
                _options.Network.Decoder = decoder;
                return this;
            }
            public ActorTlsServerOptionsBuilder UseSocketOption(SocketOption socketOption)
            {
                ArgumentNullException.ThrowIfNull(socketOption);
                _options.Network.SocketOption = socketOption;
                return this;
            }
            public ActorTlsServerOptionsBuilder WithInitialPoolSize(int size)
            {
                if (size < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(size), "InitialSessionPoolSize must be >= 0");
                }
                _options.Network.InitialSessionPoolSize = size;
                return this;
            }
            public ActorTlsServerOptionsBuilder UseTlsOptions(TlsServerOptions tlsOptions)
            {
                ArgumentNullException.ThrowIfNull(tlsOptions);
                _options.TlsOptions = tlsOptions;
                return this;
            }
            public ActorTlsServerOptionsBuilder UseCertificate(X509Certificate2 certificate)
            {
                ArgumentNullException.ThrowIfNull(certificate);
                _options.TlsOptions = new TlsServerOptions(certificate);
                return this;
            }
            public ActorTlsServerOptionsBuilder UseCertificate(
                X509Certificate2 certificate,
                SslProtocols enabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                bool clientCertificateRequired = false,
                bool checkCertificateRevocation = false,
                RemoteCertificateValidationCallback remoteCertificateValidationCallback = null)
            {
                ArgumentNullException.ThrowIfNull(certificate);
                _options.TlsOptions = new TlsServerOptions(
                    certificate,
                    enabledSslProtocols,
                    clientCertificateRequired,
                    checkCertificateRevocation,
                    remoteCertificateValidationCallback);
                return this;
            }

            public ActorTlsServerOptions Build()
            {
                return _options;
            }
        }

    }
}
