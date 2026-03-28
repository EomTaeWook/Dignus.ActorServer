# Dignus Actor Framework

[![Actor.Abstractions](https://img.shields.io/nuget/v/Dignus.Actor.Abstractions.svg?label=Actor.Abstractions)](https://www.nuget.org/packages/Dignus.Actor.Abstractions)
[![Actor.Core](https://img.shields.io/nuget/v/Dignus.Actor.Core.svg?label=Actor.Core)](https://www.nuget.org/packages/Dignus.Actor.Core)
[![ActorServer](https://img.shields.io/nuget/v/Dignus.ActorServer.svg?label=ActorServer)](https://www.nuget.org/packages/Dignus.ActorServer)

High-performance actor-based runtime and network server framework.

---

## Overview

Dignus Actor Framework provides a complete solution for building
**high-throughput, message-driven systems**.

It is designed around:

- Single-threaded actor execution
- Message-based concurrency
- Zero shared mutable state
- Minimal network overhead

---

## Architecture

```
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

- Single-threaded execution per actor
- No shared mutable state
- Dedicated dispatcher threads (no ThreadPool usage)
- Lock-free message scheduling
- Deterministic execution model

---

## Network Flow

```
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

```
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
            // handle message
        }

        return ValueTask.CompletedTask;
    }
}
```

---

## When to Use

Use this framework when you need:

- High-throughput network servers
- Actor-based concurrency model
- Deterministic execution per entity
- Clear separation between runtime and transport

---

## Performance

Network benchmarks are available here:

- [Dignus.Actor.Network Benchmark](publish/Dignus.Actor.Network.md)

---

## Summary

Dignus Actor Framework separates the actor runtime from the network layer,
allowing flexible and scalable system design.

- **Simple by default**
- **Extensible when needed**
- **Designed for performance**