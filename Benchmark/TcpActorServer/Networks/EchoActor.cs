using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network;
using Dignus.Actor.Network.Actors;
using TcpActorServer.Messages;

namespace TcpActorServer.Networks
{
    internal class EchoActor : SessionActorBase
    {
        protected override async ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if(message is BinaryMessage rawMessage)
            {
                NetworkSession.Send(rawMessage);
            }
        }
        public override void OnKill() 
        {

        }
    }
}
