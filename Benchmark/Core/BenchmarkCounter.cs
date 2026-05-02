namespace Core
{
    internal sealed class BenchmarkContext
    {
        public volatile bool IsRunning;
    }

    internal sealed class BenchmarkCounter
    {
        private readonly long _targetMessageCount;
        private readonly TaskCompletionSource<bool> _taskCompletionSource;
        private long _processedMessageCount;

        public BenchmarkCounter(long targetMessageCount)
        {
            _targetMessageCount = targetMessageCount;
            _taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public Task CompletedTask
        {
            get { return _taskCompletionSource.Task; }
        }

        public long ProcessedMessageCount
        {
            get { return Interlocked.Read(ref _processedMessageCount); }
        }

        public bool IncreaseAndCheckCompleted()
        {
            long processedMessageCount = Interlocked.Increment(ref _processedMessageCount);

            if (processedMessageCount >= _targetMessageCount)
            {
                _taskCompletionSource.TrySetResult(true);
                return true;
            }

            return false;
        }
    }

}
