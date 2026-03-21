# Dignus.Actor.Core

Core actor runtime and messaging primitives for Dignus.

---

## Overview

`Dignus.Actor.Core` provides the foundational runtime for building actor-based systems.

This package contains the core components required to run actors and process messages, including:

- Actor execution model
- Message passing infrastructure
- Actor lifecycle management
- Dispatcher-based scheduling

It does **not** include any networking or transport features.

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

Actors are distributed across dispatchers:

```text
dispatcherIndex = actorId % dispatcherCount
```

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

## What This Package Includes

- Actor runtime
- Message system (`IActorMessage`)
- Actor references (`IActorRef`)
- Dispatcher scheduling
- Actor lifecycle management

---

## What This Package Does NOT Include

- TCP / TLS networking
- Session handling
- Packet encoding/decoding
- Protocol pipelines

For networking support, use:

- `Dignus.ActorServer`

---

## Package Structure

- `Dignus.Actor.Core` → Core actor runtime
- `Dignus.ActorServer` → Network and server integration

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