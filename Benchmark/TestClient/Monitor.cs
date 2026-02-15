using Dignus.Framework;

namespace EchoClient
{
    internal class Monitor : Singleton<Monitor>
    {
        private long _totalClientCount = 0;
        private long _totalReceivedCount = 0;
        private long _totalBytes = 0;
        private long _totalSendCount = 0;
        public double MaxRttMs { get; private set; } = -1;
        public double MinRttMs { get; private set; } = 999999;

        private object _minRttSyncObj = new object();
        private object _maxRttSyncObj = new object();

        private DateTime _startDateTime = DateTime.MinValue;
        public void AddReceivedCount(long receivedCount)
        {
            Interlocked.Add(ref _totalReceivedCount, receivedCount);
        }
        public void AddTotalBytes(long totalBytes)
        {
            Interlocked.Add(ref _totalBytes, totalBytes);
        }
        public void AddTotalSendCount(long sendCount)
        {
            Interlocked.Add(ref _totalSendCount, sendCount);
        }
        public void AddClientCount(long count)
        {
            Interlocked.Add(ref _totalClientCount, count);
        }
        public void Start()
        {
            _startDateTime = DateTime.UtcNow;
        }
        public void SetMaxRttMs(double maxRttMs)
        {
            lock (_maxRttSyncObj)
            {
                if (MaxRttMs < maxRttMs)
                {
                    MaxRttMs = maxRttMs;
                }
            }
        }
        public void SetMinRttMs(double minRttMs)
        {
            lock (_minRttSyncObj)
            {
                if (MinRttMs > minRttMs)
                {
                    MinRttMs = minRttMs;
                }
            }
        }
        public void Print(string serverName)
        {
            Console.WriteLine($"[Monitor]");
            if (string.IsNullOrEmpty(serverName) == false)
            {
                Console.WriteLine($"[{serverName}]");
            }
            Console.WriteLine($"Total Client: {_totalClientCount}");
            //Console.WriteLine($"Total Bytes: {_totalBytes}");
            //Console.WriteLine($"Total Message: {_totalBytes / Program.Message.Length}");
            Console.WriteLine($"Total Received: {_totalReceivedCount:N0}");
            Console.WriteLine($"Max RTT (ms): {MaxRttMs:F2}");
            Console.WriteLine($"Min RTT (ms): {MinRttMs:F2}");
        }
        public void PrintEcho(string serverName)
        {
            Console.WriteLine($"[Monitor]");
            if (string.IsNullOrEmpty(serverName) == false)
            {
                Console.WriteLine($"[{serverName}]");
            }
            var totalSeconds = (DateTime.UtcNow - _startDateTime).TotalSeconds;

            Console.WriteLine($"Total Time: {totalSeconds:F3} seconds");
            Console.WriteLine($"Total Client: {_totalClientCount:N0}");
            Console.WriteLine($"Total Bytes: {_totalBytes:N0}");
            Console.WriteLine($"Total Data: {_totalBytes / (1024.0 * 1024.0 * 1024.0):F2} GiB");

            var totalMessage = _totalBytes / Consts.Message.Length;
            Console.WriteLine($"Total Message: {totalMessage:N0}");

            Console.WriteLine($"Data Throughput: {(_totalBytes / (1024.0 * 1024.0)) / totalSeconds:F2} MiB/s");
            Console.WriteLine($"Message Throughput: {totalMessage / totalSeconds:N0} msg/s");

        }


        public void Clear()
        {
            _totalClientCount = 0;
            _totalReceivedCount = 0;
            MaxRttMs = -1;
            MinRttMs = 999999;
        }
    }
}
