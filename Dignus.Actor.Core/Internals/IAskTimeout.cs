// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

namespace Dignus.Actor.Core.Internals
{
    internal interface IAskTimeout
    {
        long DeadlineAtTicks { get; }
        void SetTimeout();
    }
}
