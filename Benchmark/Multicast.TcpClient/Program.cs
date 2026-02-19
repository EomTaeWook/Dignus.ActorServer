using Dignus.Log;
using Dignus.Sockets;
using Multicast.TcpClient.Serializer;

namespace Multicast.TcpClient
{
    internal class Program
    {
        static SessionSetup EchoSetupFactory()
        {
            EchoPacketProcessor echoSerializer = new();

            return new SessionSetup(
                    echoSerializer,
                    echoSerializer,
                    [echoSerializer]);
        }

        private static void MulticaseBechmark()
        {
            var clients = new List<ClientModule>();
            LogHelper.Info($"start");

            var sessionConfiguration = new SessionConfiguration(EchoSetupFactory);

            sessionConfiguration.SocketOption.SendBufferSize = 65536;
            sessionConfiguration.SocketOption.MaxPendingSendBytes = int.MaxValue;

            {
                for (int i = 0; i < 100; ++i)
                {
                    try
                    {
                        var client = new ClientModule(sessionConfiguration);
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
            LogHelper.Info("Multicast benchmark started.");

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

        private const string ServerName = "DignusActorServer";

        static void Main(string[] args)
        {
            LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
            LogBuilder.Build();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            MulticaseBechmark();

            Console.ReadLine();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper.Error(e.ExceptionObject as Exception);
        }
    }
}