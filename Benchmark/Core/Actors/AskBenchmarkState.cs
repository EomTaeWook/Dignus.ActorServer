using System.Threading;

namespace Core.Actors
{
    internal sealed class AskBenchmarkState
    {
        private int _running = 1;
        private long _completedCount;

        public bool IsRunning => Volatile.Read(ref _running) == 1;
        public long CompletedCount => Interlocked.Read(ref _completedCount);

        public void Stop() => Volatile.Write(ref _running, 0);
        public void Increment() => Interlocked.Increment(ref _completedCount);
    }
}
