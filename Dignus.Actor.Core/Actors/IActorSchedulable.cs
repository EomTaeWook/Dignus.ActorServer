using System.Threading.Tasks;

namespace Dignus.Actor.Core.Actors
{
    internal interface IActorSchedulable
    {
        Task ExecuteAsync();
    }
}
