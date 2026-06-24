# Dignus Actor Framework

[![Actor.Abstractions](https://img.shields.io/nuget/v/Dignus.Actor.Abstractions.svg?label=Actor.Abstractions)](https://www.nuget.org/packages/Dignus.Actor.Abstractions)
[![Actor.Core](https://img.shields.io/nuget/v/Dignus.Actor.Core.svg?label=Actor.Core)](https://www.nuget.org/packages/Dignus.Actor.Core)
[![ActorServer](https://img.shields.io/nuget/v/Dignus.ActorServer.svg?label=ActorServer)](https://www.nuget.org/packages/Dignus.ActorServer)

High-performance actor runtime and network server framework.

---

## Overview

Dignus Actor Framework provides a complete solution for building
**high-throughput, message-driven systems**.

It is designed around:

- Single-threaded actor execution
- Message-based concurrency
- Isolated mutable actor state
- Minimal network overhead

---

## Architecture

```text
Application
   ↓
Dignus.ActorServer (Network / Session / Protocol)
   ↓
Dignus.Actor.Core (Actor Runtime / Dispatcher / Messaging)
   ↓
Dignus.Actor.Abstractions (Contracts)
```

---

## Packages

### Dignus.Actor.Abstractions

Minimal shared contracts.

Includes:

- `IActorMessage`

Used by:

- Protocol definitions
- Shared message models

---

### Dignus.Actor.Core

Core actor runtime.

Includes:

- Actor execution model
- Dispatcher scheduling
- Actor lifecycle
- Message processing
- `IActorRef`
- `IAskActorRef`
- Request/response messaging through Ask
- Dead-letter publication for undeliverable messages

Does NOT include:

- Networking
- Transport
- Protocol handling

---

### Dignus.ActorServer

Network and server layer.

Includes:

- TCP / TLS server
- Session actor model
- Packet decode / encode
- Protocol handling
- Optional protocol pipeline

---

## Core Characteristics

- Sequential execution per actor
- Isolated mutable actor state
- Actor scheduling uses dedicated dispatcher threads
- Low-overhead dispatcher scheduling
- Predictable actor-local execution
- Message-based communication between actors

---

## Network Flow

```text
TCP / TLS
   ↓
Packet Decode
   ↓
Actor Message Creation
   ↓
Actor Mailbox
   ↓
Dispatcher Execution
   ↓
Actor.OnReceive
```

---

## Protocol Model

Dignus.ActorServer uses a **direct message model by default**.

```text
Protocol → Deserialize → Actor
```

- No handler required
- No pipeline required
- Minimal overhead

An optional pipeline is available when middleware or execution control is needed.

---

## Example

```csharp
public readonly struct PingMessage : IActorMessage
{
}

public sealed class SampleActor : ActorBase
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

---

## When to Use

Use this framework when you need:

- High-throughput network servers
- Actor-based concurrency
- Sequential processing per entity
- Clear separation between runtime and transport
- Message-driven system design

---

## Send vs SendAsync

`Dignus.ActorServer` provides both synchronous and asynchronous send paths.

Both APIs use the same internal send path, so message ordering and send consistency are preserved in the same way.

The difference is how the send loop is executed.

When called from actor execution, `Send` executes the send loop directly on the actor dispatcher thread.
`SendAsync` schedules the send loop to the ThreadPool through `Task.Run`.

Because of this, `SendAsync` can move send work away from the actor thread, but it can also introduce additional scheduling overhead, allocation, and GC pressure compared with `Send`.

Use `Send` when the actor thread is allowed to execute the send loop directly.
Use `SendAsync` when the send loop should be offloaded to the ThreadPool.

---

## Performance

Benchmarks:

- [Dignus.Actor.Core Benchmark](publish/Dignus.Actor.Core.md)
- [Dignus.Actor.Network Benchmark](publish/Dignus.Actor.Network.md)

---

## Summary

Dignus Actor Framework separates the actor runtime from the network layer,
allowing flexible and scalable system design.

- **Simple by default**
- **Extensible when needed**
- **Designed for performance**
