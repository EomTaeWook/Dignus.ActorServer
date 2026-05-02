using Core.Messages;
using Dignus.Actor.Abstractions;
using Dignus.Actor.Core;
using System.Threading.Tasks;

namespace Core.Actors
{
    internal sealed class PingActor(BenchmarkContext benchmarkContext) : ActorBase
    {
        private readonly BenchmarkContext _benchmarkContext = benchmarkContext;
        private IActorRef _pongActorRef;
        private long _processedMessageCount;

        public long ProcessedMessageCount
        {
            get { return _processedMessageCount; }
        }

        public void SetPongActorRef(IActorRef pongActorRef)
        {
            _pongActorRef = pongActorRef;
        }

        protected override ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if (_benchmarkContext.IsRunning == false)
            {
                return ValueTask.CompletedTask;
            }

            if (message is PingMessage)
            {
                _processedMessageCount++;
                _pongActorRef.Post(PongMessage.Instance, Self);
            }

            return ValueTask.CompletedTask;
        }
    }
}