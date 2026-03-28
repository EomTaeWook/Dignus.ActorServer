using Dignus.Actor.Abstractions;
using Dignus.Actor.Network.Attributes;
using TlsActorServer.Networks;

namespace TcpActorServer.Messages
{
    [Protocol((int)CSProtocol.EchoMessage)]
    internal class EchoMessage : IActorMessage
    {

    }
}
