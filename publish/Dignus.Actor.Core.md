# Dignus.Actor.Core

[![NuGet](https://img.shields.io/nuget/v/Dignus.ActorServer.svg)](https://www.nuget.org/packages/Dignus.Actor.Core/)

Core actor runtime and messaging primitives for Dignus.

---

## Overview

`Dignus.Actor.Core` provides the fundamental runtime for actor-based execution.

It implements a message-driven concurrency model where actors process messages sequentially on dedicated dispatcher threads.

This package focuses purely on execution, messaging, and scheduling.

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

- Creating and managing actors
- Routing messages to actor mailboxes
- Assigning actors to dispatchers
- Controlling actor lifecycle

Actors are assigned to dispatchers using a deterministic strategy.

By default, a modulo-based distribution can be used:

```text
dispatcherIndex = actorId % dispatcherCount
```

Actors can also be assigned to a specific dispatcher directly if needed.

---

### ActorBase

Base class for all actors.

- Handles incoming messages
- Maintains actor-local state
- Executes on a single dispatcher thread

---

### IActorMessage

Marker interface for messages exchanged between actors.

- All communication is done via message passing
- No direct method calls between actors

---

### IActorRef

Reference to an actor.

- Used to send messages safely
- Hides actual actor instance
- Enables location-transparent communication

---

### Dispatcher

Execution unit of the actor system.

- Owns a dedicated thread
- Schedules actor execution
- Ensures sequential processing per actor
- Resumes async continuations on the same thread

---

## Concurrency Model

- Each actor processes messages sequentially
- No concurrent execution inside a single actor
- No shared state between actors
- Communication only through messages

This keeps actor logic simple and predictable.

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

## Summary

`Dignus.Actor.Core` is the foundational runtime for building actor-based applications with Dignus.

Use this package when you need the actor model itself.