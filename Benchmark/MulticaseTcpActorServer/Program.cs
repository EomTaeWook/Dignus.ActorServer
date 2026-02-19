// See https://aka.ms/new-console-template for more information
using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network.Messages;
using Dignus.Log;
using Multicast.TcpActorServer;

internal class Program
{
    private static void Main(string[] args)
    {
        LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
        LogBuilder.Build();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        TcpServer echoServer = new();
        echoServer.Start(5000);

        LogHelper.Info("actor server start");
        LogHelper.Info("Press any key to start multicast benchmark...");
        Console.ReadLine();

        bool multicasting = true;
        int messagesRate = 1000000;
        int messageSize = 32;

        LogHelper.Info("Multicast benchmark started.");

        var task = Task.Run(() =>
        {
            byte[] message = new byte[messageSize];
            var binaryMessage = new BinaryMessage(message);

            var sessions = new List<IActorRef>(echoServer.GetAllSessionActors());

            while (multicasting)
            {
                var start = DateTime.UtcNow;
                for (int i = 0; i < messagesRate; i++)
                {
                    foreach (var session in sessions)
                    {
                        session.Post(binaryMessage);
                    }
                }

                var end = DateTime.UtcNow;

                var milliseconds = (int)(end - start).TotalMilliseconds;
                if (milliseconds < 1000)
                {
                    Thread.Sleep(1000 - milliseconds);
                }
                else
                {
                    Thread.Yield();
                }
            }
        });


        Console.ReadLine();
        multicasting = false;

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper.Error(e.ExceptionObject as Exception);
        }
    }
}