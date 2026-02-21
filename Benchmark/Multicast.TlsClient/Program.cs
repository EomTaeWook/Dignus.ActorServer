using Dignus.Log;
using Dignus.Sockets;
using Dignus.Sockets.Tls;
using Multicast.TcpClient;
using Multicast.TcpClient.Serializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

internal class Program
{
    private const string ServerName = "DignusActorTlsServer";
    static SessionSetup EchoSetupFactory()
    {
        EchoPacketProcessor echoSerializer = new();

        return new SessionSetup(
                echoSerializer,
                echoSerializer,
                [echoSerializer]);
    }
    private static void MulticastBechmark()
    {
        var clients = new List<ClientModule>();
        LogHelper.Info($"start");

        var sessionConfiguration = new SessionConfiguration(EchoSetupFactory);

        sessionConfiguration.SocketOption.SendBufferSize = 65536;
        sessionConfiguration.SocketOption.MaxPendingSendBytes = int.MaxValue;

        var pfxPath = Path.Combine(AppContext.BaseDirectory, "client.pfx");

        X509Certificate2 clientCert = X509CertificateLoader.LoadPkcs12FromFile(pfxPath, "1234");
        var tlsOption = new TlsClientOptions("localhost", clientCert);

        {
            for (int i = 0; i < 100; ++i)
            {
                try
                {
                    var client = new ClientModule(sessionConfiguration, tlsOption);
                    clients.Add(client);

                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }

            for (int i = 0; i < 100; ++i)
            {
                try
                {
                    clients[i].Connect("127.0.0.1", 5000);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        }

        LogHelper.Info("client all connected");
        LogHelper.Info("Press any key to start benchmark...");
        Console.ReadLine();
        LogHelper.Info("Multicast Tls benchmark started.");

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

    private static void Main(string[] args)
    {
        LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
        LogBuilder.Build();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        MulticastBechmark();

        Console.ReadLine();
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogHelper.Error(e.ExceptionObject as Exception);
    }
}