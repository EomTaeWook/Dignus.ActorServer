using System.Threading;

namespace Core.Actors
{
    internal sealed class AskBenchmarkState
    {
        private int _running = 1;

        public bool IsRunning => Volatile.Read(ref _running) == 1;

        public void Stop() => Volatile.Write(ref _running, 0);
    }
}
