using Dignus.Sockets.Pipeline;
using TcpActorServer.Handlers;
using TcpActorServer.Networks;

namespace TcpActorServer.Middleware
{
    internal struct ClientContext : IPipelineContext<ClientPacketHandler, string, EchoActor>
    {
        public EchoActor State { get; set; }

        public ClientPacketHandler Handler { get; set; }

        public string Body { get; set; }

        public int Protocol { get; set; }
    }
}
