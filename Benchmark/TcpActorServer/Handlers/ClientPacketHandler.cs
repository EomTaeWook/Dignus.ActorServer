using Dignus.Sockets.Attributes;
using Dignus.Sockets.Interfaces;
using System.Text.Json;
using TcpActorServer.Networks;

namespace TcpActorServer.Handlers
{
    internal class ClientPacketHandler : IProtocolHandler<string>
    {
        public T DeserializeBody<T>(string body)
        {
            return JsonSerializer.Deserialize<T>(body);
        }

        [ProtocolName("EchoMessage")]
        public void Process(EchoActor echoActor, EchoMessage echoMessage)
        {
        }
    }
}
