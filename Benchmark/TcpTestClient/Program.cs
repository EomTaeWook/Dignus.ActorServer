using Dignus.Log;
using Dignus.Sockets;
using TcpEchoClient.Handler;
using TcpEchoClient.Protocol;
using TcpEchoClient.Serializer;

namespace TcpEchoClient
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

        private static void RoundTripBechmark(int clientCount)
        {
            var clients = new List<ClientModule>();
            LogHelper.Info($"start");

            var sessionConfiguration = new SessionConfiguration(EchoSetupFactory);

            sessionConfiguration.SocketOption.SendBufferSize = 65536;
            sessionConfiguration.SocketOption.MaxPendingSendBytes = int.MaxValue;

            {
                for (int i = 0; i < clientCount; ++i)
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

                foreach(var client in clients)
                {
                    client.Connect("127.0.0.1", 5000);
                    client.SendMessage(Consts.Message, 1000);
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

        private const string ServerName = "DignusActorServer";

        static void Main(string[] args)
        {
            LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
            LogBuilder.Build();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //EchoTest
            RoundTripBechmark(1);

            Console.ReadLine();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper.Error(e.ExceptionObject as Exception);
        }
    }
}