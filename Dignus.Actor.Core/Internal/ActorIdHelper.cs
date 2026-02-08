// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

namespace Dignus.Actor.Core.Internal
{
    internal class ActorIdHelper
    {
        private const int DispatcherBitShift = 40;
        private const int ServerBitShift = 48;

        private const long SequenceMask = 0xFFFFFFFFFFL; // 40비트
        private const long DispatcherMask = 0xFFL;       // 8비트
        private const long ServerMask = 0xFFFFL;         // 16비트

        public static long CreateId(int serverId, int dispatcherId, long sequence)
        {
            return ((long)(serverId & ServerMask) << ServerBitShift) |
                   ((long)(dispatcherId & DispatcherMask) << DispatcherBitShift) |
                   (sequence & SequenceMask);
        }

        public static int GetServerId(long actorId)
        {
            return (int)((actorId >> ServerBitShift) & ServerMask);
        }

        public static int GetDispatcherId(long actorId)
        {
            return (int)((actorId >> DispatcherBitShift) & DispatcherMask);
        }

        public static long GetSequence(long actorId)
        {
            return actorId & SequenceMask;
        }
    }
}
