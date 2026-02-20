using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Messages;
using Dignus.Actor.Network;
using Dignus.Actor.Network.Actors;
using Dignus.Actor.Network.Messages;

namespace Multicast.TcpActorServer.Networks
{
    internal class EchoActor : SessionActorBase
    {
        public EchoActor()
        {
        }

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
