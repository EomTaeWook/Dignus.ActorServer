using Dignus.Actor.Network.Messages;
using System;

namespace TcpActorServer.Messages
{
    public class BinaryMessage(ArraySegment<byte> bytes) : INetworkActorMessage
    {
        public ArraySegment<byte> Data { get; } = bytes;
    }
}
