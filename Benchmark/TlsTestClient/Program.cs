using Dignus.Log;
using Dignus.Sockets;
using Dignus.Sockets.Tls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TlsTestClient;
using TlsTestClient.Serializer;

internal class Program
{
    static SessionSetup EchoSetupFactory()
    {
        EchoPacketProcessor echoPacketProcessor = new();

        return new SessionSetup(
                echoPacketProcessor,
                echoPacketProcessor,
                [echoPacketProcessor]);
    }

    private static void SingleBechmark()
    {
        var clients = new List<ClientModule>();
        LogHelper.Info($"start");

        var sessionConfiguration = new SessionConfiguration(EchoSetupFactory);

        sessionConfiguration.SocketOption.SendBufferSize = 65536;
        sessionConfiguration.SocketOption.MaxPendingSendBytes = int.MaxValue;

        {
            var pfxPath = Path.Combine(AppContext.BaseDirectory, "client.pfx");

            X509Certificate2 clientCert = X509CertificateLoader.LoadPkcs12FromFile(pfxPath, "1234");

            var tlsOption = new TlsClientOptions("localhost", clientCert);

            var client = new ClientModule(sessionConfiguration, tlsOption);

            try
            {
                client.Connect("127.0.0.1", 5000);
                clients.Add(client);
                client.SendMessage(Consts.Message, 1000);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        Monitor.Instance.Start();
        Task.Delay(10000).GetAwaiter().GetResult();

        {
            foreach (var client in clients)
            {
                client.Close();
            }
            Monitor.Instance.PrintEcho(ServerName);
        }

    }

    private const string ServerName = "DignusActorTlsServer";

    static void Main(string[] args)
    {
        LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
        LogBuilder.Build();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        //EchoTest
        SingleBechmark();

        Console.ReadLine();
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogHelper.Error(e.ExceptionObject as Exception);
    }
}
