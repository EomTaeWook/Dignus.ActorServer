// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using System;

namespace Dignus.Actor.Network.Options
{
    public sealed class ActorSystemOptions
    {
        public int DispatcherThreadCount { get; set; } = Environment.ProcessorCount;
    }
}
