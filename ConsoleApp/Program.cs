// See https://aka.ms/new-console-template for more information
using ConsoleApp;
using Dignus.Log;

LogBuilder.Configuration(LogConfigXmlReader.Load($"{AppContext.BaseDirectory}DignusLog.config"));
LogBuilder.Build();

AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

EchoServer echoServer = new EchoServer();
echoServer.Start(5000);

LogHelper.Info("actor server start");


Console.Read();


void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    LogHelper.Error(e.ExceptionObject as Exception);
}

