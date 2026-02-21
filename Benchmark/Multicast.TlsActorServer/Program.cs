// See https://aka.ms/new-console-template for more information
using Dignus.Actor.Network.Options;
using Dignus.Log;
using Multicast.TlsActorServer;
using Multicast.TlsActorServer.Networks;
using Multicast.TlsActorServer.Networks.PacketFramer;
using System.Security.Cryptography.X509Certificates;

internal class Program
{
    private static void Main(string[] args)
    {
        LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
        LogBuilder.Build();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        var pfxPath = Path.Combine(AppContext.BaseDirectory, "server.pfx");

        X509Certificate2 serverCert = X509CertificateLoader.LoadPkcs12FromFile(pfxPath, "1234");

        var option = TlsServerOptions.Builder()
            .UseDecoder(new MyPacketFramer())
            .UseSerializer(new MessageSerializer())
            .UseCertificate(serverCert)
            .Build();

        option.Network.MailboxCapacity = 65535;

        TlsServer echoServer = new(option);
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