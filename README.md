# Dignus Actor Framework

[![Actor.Abstractions](https://img.shields.io/nuget/v/Dignus.Actor.Abstractions.svg?label=Actor.Abstractions)](https://www.nuget.org/packages/Dignus.Actor.Abstractions)
[![Actor.Core](https://img.shields.io/nuget/v/Dignus.Actor.Core.svg?label=Actor.Core)](https://www.nuget.org/packages/Dignus.Actor.Core)
[![ActorServer](https://img.shields.io/nuget/v/Dignus.ActorServer.svg?label=ActorServer)](https://www.nuget.org/packages/Dignus.ActorServer)

High-performance actor-based runtime and network server framework built on top of Dignus.

---

## Overview

Dignus Actor Framework is composed of three packages:

- `Dignus.Actor.Abstractions` → Minimal shared contracts (`IActorMessage`)
- `Dignus.Actor.Core` → Actor runtime and messaging primitives
- `Dignus.ActorServer` → Network transport and server integration

The framework follows a strict message-driven concurrency model with dedicated dispatcher threads and no shared mutable state.

---

## Architecture

```
Application
   ↓
Dignus.ActorServer (Network / Protocol / Session)
   ↓
Dignus.Actor.Core (Actor runtime / Dispatcher / Messaging)
   ↓
Dignus.Actor.Abstractions (Shared contracts)
```

---

## Packages

### Dignus.Actor.Abstractions

Minimal shared contracts.

Includes:

- IActorMessage

This package allows protocol and model libraries to integrate with the actor system without referencing the full runtime.

---

### Dignus.Actor.Core

Core actor runtime.

Includes:

- Actor execution model
- Message system (IActorMessage)
- Actor references (IActorRef)
- Dispatcher scheduling
- Actor lifecycle management

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
- Protocol pipeline
- Middleware execution

---

## Performance

Network performance benchmarks are documented separately:

- [Dignus.Actor.Network Benchmark](publish/Dignus.Actor.Network.md)

---

## Design Goals

- Single-threaded execution per actor
- Message-driven concurrency model
- No shared mutable state
- Dedicated dispatcher threads
- High-throughput network processing
- Clear separation of runtime and transport

---

## Core Execution Model

Actors are distributed across dispatchers:

```text
dispatcherIndex = actorId % dispatcherCount
```

Each actor:

- Processes messages sequentially
- Executes on a dedicated dispatcher thread
- Resumes async continuations on the same thread

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
Protocol Pipeline
   ↓
Handler Execution
```

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
        }

        return ValueTask.CompletedTask;
    }
}
```

---

## When to Use

- Use `Dignus.Actor.Core` when you only need the actor runtime
- Use `Dignus.ActorServer` when you need networking and protocol handling

---

## Summary

Dignus Actor Framework provides a complete solution for building high-performance message-driven systems.

It separates the actor runtime from the network layer, allowing flexible and scalable system design.