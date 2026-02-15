// See https://aka.ms/new-console-template for more information
using Dignus.Log;
using System.Security.Cryptography.X509Certificates;
using TlsActorServer;

LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
LogBuilder.Build();

AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

var pfxPath = Path.Combine(AppContext.BaseDirectory, "server.pfx");
X509Certificate2 serverCert = X509CertificateLoader.LoadPkcs12FromFile(pfxPath, "1234");

TlsServer echoServer = new(serverCert);
echoServer.Start(5000);

LogHelper.Info("tls actor server start");

Console.Read();


static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    LogHelper.Error(e.ExceptionObject as Exception);
}

