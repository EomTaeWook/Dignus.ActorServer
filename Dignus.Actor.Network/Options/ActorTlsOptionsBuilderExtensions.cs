// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Dignus.Actor.Network.Options
{
    public static class ActorTlsOptionsBuilderExtensions
    {
        public static TBuilder UseTlsOptions<TBuilder>(this TBuilder builder, Sockets.Tls.TlsServerOptions tlsOptions)
            where TBuilder : IActorOptionsBuilderBase<TlsServerOptions>
        {
            ArgumentNullException.ThrowIfNull(tlsOptions);
            builder.Options.TlsOptions = tlsOptions;
            return builder;
        }

        public static TBuilder UseCertificate<TBuilder>(this TBuilder builder, X509Certificate2 certificate)
            where TBuilder : IActorOptionsBuilderBase<TlsServerOptions>
        {
            ArgumentNullException.ThrowIfNull(certificate);
            builder.Options.TlsOptions = new Sockets.Tls.TlsServerOptions(certificate);
            return builder;
        }

        public static TBuilder UseCertificate<TBuilder>(
            this TBuilder builder,
            X509Certificate2 certificate,
            SslProtocols enabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            bool clientCertificateRequired = false,
            bool checkCertificateRevocation = false,
            RemoteCertificateValidationCallback remoteCertificateValidationCallback = null)
            where TBuilder : IActorOptionsBuilderBase<TlsServerOptions>
        {
            ArgumentNullException.ThrowIfNull(certificate);

            builder.Options.TlsOptions = new Sockets.Tls.TlsServerOptions(
                certificate,
                enabledSslProtocols,
                clientCertificateRequired,
                checkCertificateRevocation,
                remoteCertificateValidationCallback);

            return builder;
        }
    }
}
