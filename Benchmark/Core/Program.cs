using Core;
using Core.Actors;
using Core.Messages;
using Dignus.Actor.Core;
using Dignus.Log;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        InitializeLog();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        //await RunPostBenchmarkAsync();
        await RunAskBenchmarkAsync();

        Console.ReadKey();
    }

    private static void InitializeLog()
    {
        LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
        LogBuilder.Build();
    }

    private static async Task RunPostBenchmarkAsync()
    {
        int actorPairCount = 348;
        int pipelineSizePerPair = 1000;
        int benchmarkSeconds = 10;

        var actorSystem = new ActorSystem();
        actorSystem.OnDeadLetterDetected += ActorSystem_OnDeadLetterDetected;

        var benchmarkContext = new BenchmarkContext();

        var pingActors = new List<PingActor>(actorPairCount);
        var pongActors = new List<PongActor>(actorPairCount);
        var pingActorRefs = new List<IActorRef>(actorPairCount);

        CreatePingPongActors(
            actorSystem,
            benchmarkContext,
            actorPairCount,
            pingActors,
            pongActors,
            pingActorRefs);

        benchmarkContext.IsRunning = true;

        var stopwatch = Stopwatch.StartNew();

        PostInitialPingMessages(
            pingActorRefs,
            pipelineSizePerPair);

        await Task.Delay(benchmarkSeconds * 1000);

        benchmarkContext.IsRunning = false;
        stopwatch.Stop();

        long processedMessageCount = GetProcessedMessageCount(pingActors, pongActors);
        double messagesPerSecond = processedMessageCount / stopwatch.Elapsed.TotalSeconds;

        Console.WriteLine($"Actor Pair Count: {actorPairCount:N0}");
        Console.WriteLine($"Pipeline Size Per Pair: {pipelineSizePerPair:N0}");
        Console.WriteLine($"Processed Messages: {processedMessageCount:N0}");
        Console.WriteLine($"Elapsed: {stopwatch.Elapsed.TotalSeconds:F3} sec");
        Console.WriteLine($"Throughput: {messagesPerSecond:N0} msg/s");
    }

    private static void CreatePingPongActors(
        ActorSystem actorSystem,
        BenchmarkContext benchmarkContext,
        int actorPairCount,
        List<PingActor> pingActors,
        List<PongActor> pongActors,
        List<IActorRef> pingActorRefs)
    {
        for (int actorPairIndex = 0; actorPairIndex < actorPairCount; actorPairIndex++)
        {
            var pingActor = new PingActor(benchmarkContext);
            var pongActor = new PongActor(benchmarkContext);

            var pingActorRef = actorSystem.Spawn(() => pingActor, mailboxCapacity: 2048);
            var pongActorRef = actorSystem.Spawn(() => pongActor, mailboxCapacity: 2048);

            pingActor.SetPongActorRef(pongActorRef);
            pongActor.SetPingActorRef(pingActorRef);

            pingActors.Add(pingActor);
            pongActors.Add(pongActor);
            pingActorRefs.Add(pingActorRef);
        }
    }

    private static void PostInitialPingMessages(
        List<IActorRef> pingActorRefs,
        int pipelineSizePerPair)
    {
        foreach (var pingActorRef in pingActorRefs)
        {
            for (int pipelineIndex = 0; pipelineIndex < pipelineSizePerPair; pipelineIndex++)
            {
                pingActorRef.Post(PingMessage.Instance);
            }
        }
    }

    private static long GetProcessedMessageCount(
        List<PingActor> pingActors,
        List<PongActor> pongActors)
    {
        long processedMessageCount = 0;

        foreach (var pingActor in pingActors)
        {
            processedMessageCount += pingActor.ProcessedMessageCount;
        }

        foreach (var pongActor in pongActors)
        {
            processedMessageCount += pongActor.ProcessedMessageCount;
        }

        return processedMessageCount;
    }

    private static async Task RunAskBenchmarkAsync()
    {
        int actorCount = 32;
        int pipelineSizePerActor = 1024;
        int benchmarkSeconds = 10;

        var actorSystem = new ActorSystem();
        var askActorRefs = new List<IAskActorRef>(actorCount);

        CreateAskActors(
            actorSystem,
            actorCount,
            askActorRefs);

        long completedCount = 0;
        bool isRunning = true;

        var stopwatch = Stopwatch.StartNew();

        var tasks = CreateAskBenchmarkTasks(
            askActorRefs,
            pipelineSizePerActor,
            () => Volatile.Read(ref isRunning),
            () => Interlocked.Increment(ref completedCount));

        await Task.Delay(benchmarkSeconds * 1000);

        Volatile.Write(ref isRunning, false);

        await Task.WhenAll(tasks);

        stopwatch.Stop();

        Console.WriteLine($"Actor Count: {actorCount:N0}");
        Console.WriteLine($"Pipeline Size Per Actor: {pipelineSizePerActor:N0}");
        Console.WriteLine($"Completed Ask Count: {completedCount:N0}");
        Console.WriteLine($"Elapsed: {stopwatch.Elapsed.TotalSeconds:F3} sec");
        Console.WriteLine($"Throughput: {completedCount / stopwatch.Elapsed.TotalSeconds:N0} ask/s");
    }

    private static void CreateAskActors(
        ActorSystem actorSystem,
        int actorCount,
        List<IAskActorRef> askActorRefs)
    {
        for (int actorIndex = 0; actorIndex < actorCount; actorIndex++)
        {
            var askBenchmarkActor = new AskBenchmarkActor();
            var askBenchmarkActorRef = actorSystem.Spawn(() => askBenchmarkActor, mailboxCapacity: 4096);

            askActorRefs.Add(askBenchmarkActorRef!);
        }
    }
    private static List<Task> CreateAskBenchmarkTasks(
        List<IAskActorRef> askActorRefs,
        int pipelineSizePerActor,
        Func<bool> isRunning,
        Action addCompletedCount)
    {
        var tasks = new List<Task>();

        foreach (var askActorRef in askActorRefs)
        {
            for (int pipelineIndex = 0; pipelineIndex < pipelineSizePerActor; pipelineIndex++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (isRunning())
                    {
                        var response = await askActorRef.Ask<AskPongMessage>(
                            new AskPingMessage(),
                            3000);

                        if (response.Ok)
                        {
                            addCompletedCount();
                        }
                    }
                }));
            }
        }
        return tasks;
    }

    private static void ActorSystem_OnDeadLetterDetected(Dignus.Actor.Core.DeadLetter.DeadLetterMessage deadLetterMessage)
    {
        if (deadLetterMessage.Reason == Dignus.Actor.Core.DeadLetter.DeadLetterReason.MailboxFull)
        {
            LogHelper.Error($"mailbox full");
        }
    }
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs exceptionEventArgs)
    {
        LogHelper.Error(exceptionEventArgs.ExceptionObject as Exception);
    }
}