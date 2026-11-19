# Dignus.ActorServer

English | [한국어](README.ko.md)

Actor runtime and network server libraries with actor-owned state, sequential messaging, and TCP/TLS transports.

## Requirements

Actor.Core targets netstandard2.1 and .NET 8–10. The network package targets .NET 8–10. Consult each project for contract-package targets.

## Installation

```xml
<PackageReference Include="Dignus.Actor.Core" Version="2.1.12" />
<PackageReference Include="Dignus.ActorServer" Version="3.1.12" />
```

## Quick start

This is an actor/message declaration example. Follow the core guide for actor-system setup, spawning, posting, and shutdown. The network package is optional for local actors.

```csharp
using Dignus.Actor.Abstractions;
using Dignus.Actor.Core;
using System.Threading.Tasks;

public readonly struct PingMessage : IActorMessage
{
}

public class SampleActor : ActorBase
{
    protected override ValueTask OnReceive(IActorMessage message, IActorRef sender)
    {
        if (message is PingMessage)
        {
            // Handle the message.
        }
        return ValueTask.CompletedTask;
    }
}
```

## Usage

Keep mutable state on its owning actor context. The guides cover Ask, dispatcher transitions, session actors, protocols, and Send versus SendAsync.

## Documentation

- [Overview and transport behavior](docs/Usage.md)
- [Actor contracts](publish/Dignus.Actor.Abstractions.md)
- [Actor runtime API](publish/Dignus.Actor.Core.md)
- [Network API](publish/Dignus.Actor.Network.md)

## License

MIT License. See [LICENSE](LICENSE).
