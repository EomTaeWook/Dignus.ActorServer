// See https://aka.ms/new-console-template for more information
using Dignus.Actor.Core.Actors;
using Dignus.Actor.Network.Actors;
using Dignus.Actor.Network.Messages;
using Dignus.Actor.Network.Options;
using Dignus.Log;
using Multicast.TcpActorServer;
using Multicast.TcpActorServer.Networks;
using Multicast.TcpActorServer.Networks.PacketFramer;

internal class Program
{
    private static void Main(string[] args)
    {
        LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
        LogBuilder.Build();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        var option = ServerOptions.Builder()
            .UseDecoder(new MyPacketFramer())
            .UseSerializer(new MessageSerializer()).Build();

        option.Network.MailboxCapacity = 65535;

        TcpServer echoServer = new(option);
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

            while (multicasting)
            {
                var start = DateTime.UtcNow;
                for (int i = 0; i < messagesRate; i++)
                {
                    echoServer.Broadcast(message);
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