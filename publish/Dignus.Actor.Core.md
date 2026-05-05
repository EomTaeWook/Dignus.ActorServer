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
- Request/response messaging through Ask

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
- exposes `Self` as an `IAskActorRef`

---

### IActorMessage

Marker interface for messages exchanged between actors.

- represents a message that can be sent between actors
- all actor communication is done through messages
- has no behavior

---

### IActorRef

Reference to an actor.

- used to send messages
- hides the actual actor instance
- enables safe communication

---

### IAskActorRef

Reference to an actor that supports request/response messaging.

- extends `IActorRef`
- sends Ask request messages
- waits for a response message
- intended for low-frequency request/response flows

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
IAskActorRef actorRef = actorSystem.Spawn<SampleActor>();
```

This will:

- create the actor
- assign a unique actor id
- automatically select a dispatcher
- register the actor
- return an `IAskActorRef`

Dispatcher selection is based on:

```text
dispatcherIndex = actorId % dispatcherCount
```

---

### Spawn Actor (Explicit Dispatcher)

```csharp
IAskActorRef actorRef = actorSystem.SpawnOnDispatcher<SampleActor>(0);
```

Use this when the actor must run on a specific dispatcher.

---

### Spawn with Factory

```csharp
IAskActorRef actorRef = actorSystem.Spawn(() => new SampleActor());

IAskActorRef actorRef2 = actorSystem.SpawnOnDispatcher(
    () => new SampleActor(),
    0);
```

---

### Alias Registration

```csharp
IAskActorRef actorRef = actorSystem.Spawn<SampleActor>(alias: "sample");
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
IAskActorRef actorRef = actorSystem.Spawn<SampleActor>(
    alias: "sample",
    mailboxCapacity: 2048);
```

---

## Sending Messages

Actors communicate only through messages.

`Post` sends a message to an actor without waiting for a response.

The message is enqueued into the target actor mailbox and processed later by the actor dispatcher.

Use `Post` when the caller does not need a return value.

```csharp
actorRef.Post(new PingMessage());
```

Messages must implement:

```csharp
public readonly struct PingMessage : IActorMessage
{
}
```

`Post` is fire-and-forget.

The caller does not receive a result from the target actor, and the call only represents message delivery to the actor reference.

If the target actor needs to send another message later, it should do so explicitly through another actor reference.

---

## Ask Request/Response

`Ask` is used when the caller needs a response from an actor.

Use `Post` for fire-and-forget messages when no response is needed.  
Use `Ask` when the caller must wait for a response from the target actor.

```csharp
CreateRoomResponse response = await actorRef.AskAsync<CreateRoomResponse>(
    new CreateRoomRequest(),
    3000);
```

Ask does not require request or response messages to expose a request id.

Messages only need to implement `IActorMessage`.

```csharp
public sealed class CreateRoomRequest : IActorMessage
{
    public long RoomNumber { get; set; }
}

public sealed class CreateRoomResponse : IActorMessage
{
    public bool Ok { get; set; }
    public long RoomNumber { get; set; }
}
```

When handling an Ask request, the receiving actor should send the response message back to `sender`.

```csharp
sender.Post(new CreateRoomResponse()
{
    Ok = true,
    RoomNumber = request.RoomNumber
}, Self);
```

The Ask runtime internally tracks the request and completes the waiting task when the response message is posted to the Ask reply reference.

The response type must match the type requested by `AskAsync<TResponse>`.

`Ask` is intended for control-flow operations such as:

- room creation
- database queries
- server-side commands
- management requests

Do not use `Ask` for high-frequency game-loop messages.

---

## Post vs Ask

Use `Post` when the caller only needs to send a message.

Use `Ask` when the caller needs to wait for a response.

```text
Post
caller -> target actor mailbox
caller continues immediately
```

```text
Ask
caller -> target actor mailbox
caller waits for response task
target actor -> sender.Post(response, Self)
```

In most actor flows, prefer `Post`.

Use `Ask` only when the response is required for the next control-flow step.

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

## Ask Example

```csharp
public sealed class CreateRoomRequest : IActorMessage
{
    public long RoomNumber { get; set; }
}

public sealed class CreateRoomResponse : IActorMessage
{
    public bool Ok { get; set; }
    public long RoomNumber { get; set; }
}
```

```csharp
public sealed class RoomManagerActor : ActorBase
{
    protected override ValueTask OnReceive(IActorMessage message, IActorRef sender)
    {
        if (message is CreateRoomRequest request)
        {
            sender.Post(new CreateRoomResponse()
            {
                Ok = true,
                RoomNumber = request.RoomNumber
            }, Self);
        }

        return ValueTask.CompletedTask;
    }
}
```

```csharp
CreateRoomResponse response = await roomManagerActorRef.AskAsync<CreateRoomResponse>(
    new CreateRoomRequest()
    {
        RoomNumber = 1
    },
    3000);
```

---

## Benchmark

Local in-process ping-pong benchmark.

Test environment:

```text
CPU: Intel Core i5-12400F
RAM: 32 GB
OS: Windows x64
```

Benchmark conditions:

```text
Actor Pair Count: 348
Actual Actor Count: 696
Pipeline Size Per Pair: 1,000
Benchmark Duration: 10 seconds
Counter: per-actor local counter, summed after completion
```

Best observed result:

```text
Processed Messages: 2,907,908,768
Elapsed: 10.475 sec
Throughput: 277,599,493 msg/s
```

Representative result:

```text
Throughput: around 250M ~ 270M msg/s
```

Notes:

- This benchmark measures local actor message throughput only.
- It does not include network, serialization, database access, logging, or game logic.
- Per-message global synchronization was intentionally avoided.
- Results may vary depending on CPU scheduling, background processes, power mode, GC timing, and runtime warm-up.

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
- `IAskActorRef` → sends request/response messages
- `Dispatcher` → executes actors
- `IActorMessage` → defines actor messages

---

## When to Use

Use `Dignus.Actor.Core` when you need:

- actor-based concurrency
- deterministic execution
- message-driven architecture
- isolation between components
- request/response messaging between actors
