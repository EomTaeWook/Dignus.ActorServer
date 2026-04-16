# Dignus.Actor.Core

[![NuGet](https://img.shields.io/nuget/v/Dignus.Actor.Core.svg)](https://www.nuget.org/packages/Dignus.Actor.Core/)

Core actor runtime and messaging primitives for Dignus.

---

## Overview

`Dignus.Actor.Core` provides the fundamental runtime for actor-based execution.

It implements a message-driven concurrency model where actors process messages sequentially on dedicated dispatcher threads.

This package focuses purely on:

- execution
- messaging
- scheduling

---

## Scope

`Dignus.Actor.Core` includes only the actor runtime.

Networking and server features are provided by:

- `Dignus.ActorServer`

---

## Design Goals

- Single-threaded logical execution per actor
- Message-driven concurrency model
- No shared mutable state between actors
- Deterministic scheduling via dispatcher
- Lightweight and high-performance runtime

---

## Core Components

### ActorSystem

Responsible for:

- creating and registering actors
- routing messages to actor mailboxes
- assigning actors to dispatchers
- controlling actor lifecycle

---

### ActorBase

Base class for all actors.

- processes incoming messages
- maintains actor-local state
- executes on a single dispatcher thread

---

### IActorMessage

Marker interface for messages exchanged between actors.

- all communication is done via message passing
- no direct method calls between actors

---

### IActorRef

Reference to an actor.

- used to send messages
- hides the actual actor instance
- enables safe communication

---

### Dispatcher

Execution unit of the actor system.

- owns a dedicated thread
- schedules actor execution
- ensures sequential processing per actor
- resumes async continuations on the same thread

---

## Concurrency Model

- each actor processes messages sequentially
- no concurrent execution inside a single actor
- no shared state between actors
- communication only through messages

This keeps actor logic simple and predictable.

---

## Creating and Registering Actors

Actors are created and registered through `ActorSystem`.

### Create ActorSystem

```csharp
var actorSystem = new ActorSystem();
```

By default, the number of dispatcher threads equals `Environment.ProcessorCount`.

---

### Spawn Actor (Auto Dispatcher)

```csharp
IActorRef actorRef = actorSystem.Spawn<SampleActor>();
```

This will:

- create the actor
- assign a unique actor id
- automatically select a dispatcher
- register the actor
- return an `IActorRef`

Dispatcher selection is based on:

```text
dispatcherIndex = actorId % dispatcherCount
```

---

### Spawn Actor (Explicit Dispatcher)

```csharp
IActorRef actorRef = actorSystem.SpawnOnDispatcher<SampleActor>(0);
```

Use this when the actor must run on a specific dispatcher.

---

### Spawn with Factory

```csharp
IActorRef actorRef = actorSystem.Spawn(() => new SampleActor());

IActorRef actorRef2 = actorSystem.SpawnOnDispatcher(
    () => new SampleActor(),
    0);
```

---

### Alias Registration

```csharp
IActorRef actorRef = actorSystem.Spawn<SampleActor>(alias: "sample");
```

Resolve later:

```csharp
if (actorSystem.TryGetActorRef("sample", out var actorRef))
{
}
```

---

### Mailbox Capacity

```csharp
IActorRef actorRef = actorSystem.Spawn<SampleActor>(
    alias: "sample",
    mailboxCapacity: 2048);
```

---

## Sending Messages

Actors communicate only through messages.

```csharp
actorRef.Tell(new PingMessage());
```

Messages must implement:

```csharp
public readonly struct PingMessage : IActorMessage
{
}
```

---

## Actor Example

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

## Lifecycle

- actor is created via `Spawn`
- messages are queued in mailbox
- dispatcher executes messages sequentially
- actor is removed when killed

---

## Summary

- `ActorSystem` → creates and manages actors  
- `ActorBase` → implements actor logic  
- `IActorRef` → sends messages  
- `Dispatcher` → executes actors  
- `IActorMessage` → defines messages  

---

## When to Use

Use `Dignus.Actor.Core` when you need:

- actor-based concurrency
- deterministic execution
- message-driven architecture
- isolation between components

---