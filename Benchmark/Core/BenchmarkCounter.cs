namespace Core
{
    internal sealed class BenchmarkContext
    {
        public volatile bool IsRunning;
    }

    internal sealed class BenchmarkCounter(long targetMessageCount)
    {
        private readonly long _targetMessageCount = targetMessageCount;
        private readonly TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private long _processedMessageCount;

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
