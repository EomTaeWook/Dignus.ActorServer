using Dignus.Actor.Core.Actors;

namespace Dignus.Actor.Core
{
    internal interface IActorRefProvider
    {
        bool TryGetActorRef(int id, out IActorRef actorRef);

        bool TryGetActorRef(string alias, out IActorRef actorRef);
    }
}
