using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network.Messages;
using Dignus.Sockets.Interfaces;

namespace Dignus.Actor.Network.Actors
{
    public interface INetworkSessionRef : IActorRef, INetworkSession
    {
    }
}
