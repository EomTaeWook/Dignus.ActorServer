using Core.Messages;
using Dignus.Actor.Abstractions;
using Dignus.Actor.Core;
using System.Threading.Tasks;

namespace Core.Actors
{
    internal sealed class PongActor : ActorBase
    {
        private readonly BenchmarkContext _benchmarkContext;
        private IActorRef _pingActorRef;
        private long _processedMessageCount;

        public long ProcessedMessageCount
        {
            get { return _processedMessageCount; }
        }

        public PongActor(BenchmarkContext benchmarkContext)
        {
            _benchmarkContext = benchmarkContext;
        }

        public void SetPingActorRef(IActorRef pingActorRef)
        {
            _pingActorRef = pingActorRef;
        }

        protected override ValueTask OnReceive(IActorMessage message, IActorRef sender)
        {
            if (_benchmarkContext.IsRunning == false)
            {
                return ValueTask.CompletedTask;
            }

            if (message is PongMessage)
            {
                _processedMessageCount++;
                _pingActorRef.Post(PingMessage.Instance, Self);
            }

            return ValueTask.CompletedTask;
        }
    }
}