using Dignus.Actor.Core;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network;
using TcpActorServer.Messages;

namespace TcpActorServer.Networks
{
    internal class EchoActor : SessionActorBase
    {
        protected override async ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if(message is BinaryMessage rawMessage)
            {
                NetworkSession.SendAsync(rawMessage);
            }
        }
        public override void OnKill() 
        {

        }
    }
}
