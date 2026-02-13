using Dignus.Actor.Core.Messages;

namespace ConsoleApp.Messages
{
    internal class JsonMessage : IActorMessage
    {
        public string Body { get; set; }
    }
}
