using Dignus.Log;
using Dignus.Sockets;
using EchoClient.Handler;
using EchoClient.Protocol;
using EchoClient.Serializer;

namespace EchoClient
{
    internal class Program
    {
        static SessionSetup EchoSetupFactory()
        {
            EchoPacketHandler echoSerializer = new();

            return new SessionSetup(
                    echoSerializer,
                    echoSerializer,
                    [echoSerializer]);
        }

        private static void SingleBechmark()
        {
            var clients = new List<ClientModule>();
            LogHelper.Info($"start");

            var sessionConfiguration = new SessionConfiguration(EchoSetupFactory);

            sessionConfiguration.SocketOption.SendBufferSize = 65536;
            sessionConfiguration.SocketOption.MaxPendingSendBytes = int.MaxValue;

            {
                var client = new ClientModule(sessionConfiguration);

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

        private const string ServerName = "DignusActorServer";

        static void Main(string[] args)
        {
            LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
            LogBuilder.Build();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ProtocolHandlerMapper<EchoHandler, string>.BindProtocol<SCProtocol>();
            //EchoTest
            SingleBechmark();

            Console.ReadLine();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper.Error(e.ExceptionObject as Exception);
        }
    }
}