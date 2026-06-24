# Dignus.Actor.Core

[![NuGet](https://img.shields.io/nuget/v/Dignus.Actor.Core.svg)](https://www.nuget.org/packages/Dignus.Actor.Core/)

Core actor runtime and messaging primitives for Dignus.

---

## Overview

`Dignus.Actor.Core` provides the fundamental runtime for actor-based execution.

Actors process messages sequentially on dedicated dispatcher threads. Mutable state is owned by the actor that processes it, while actors communicate through messages.

This package focuses on:

- execution
- messaging
- scheduling
- actor lifecycle

---

## Scope

`Dignus.Actor.Core` contains the actor runtime only.

Networking and server features are provided by:

- `Dignus.ActorServer`

---

## Design Goals

- Sequential logical execution per actor
- Message-driven concurrency
- Isolated mutable actor state
- Predictable actor-local execution
- Low-overhead dispatcher scheduling
- Lightweight, high-performance runtime
- Request/response messaging through Ask

---

## Core Components

### ActorSystem

Responsible for:

- creating and registering actors
- routing messages to actor mailboxes
- assigning actors to dispatchers
- resolving actor references by alias
- controlling actor lifecycle
- publishing dead letters for undeliverable messages

---

### ActorBase

Base class for all actors.

- processes incoming messages
- maintains actor-local state
- executes on its assigned dispatcher
- exposes `Self` as an `IAskActorRef`
- provides `VerifyContext()` for validating dispatcher affinity when needed

---

### IActorMessage

Marker interface for messages exchanged between actors.

- represents a message sent between actors
- carries application-defined data
- has no required behavior

---

### IActorRef

Reference to an actor.

- sends messages through `Post`
- hides the actor instance
- supports actor termination through `Kill`
- enables message-based communication without direct actor access

---

### IAskActorRef

Reference to an actor that supports request/response messaging.

- extends `IActorRef`
- sends Ask request messages
- provides an awaitable response operation
- is intended for low-frequency request/response flows

---

### Dispatcher

Execution unit of the actor system.

- owns a dedicated worker thread
- schedules actor execution
- ensures an actor is not executed concurrently
- runs continuations that capture the dispatcher synchronization context on the dispatcher thread

Continuations that explicitly avoid the captured context, such as with `ConfigureAwait(false)`, are not guaranteed to resume on the actor dispatcher thread.

---

## Concurrency Model

- each actor processes one message at a time
- an actor does not execute concurrently with itself
- mutable actor state remains isolated to that actor's execution
- actor-to-actor communication is performed through messages

This keeps actor logic focused on local state and makes execution behavior easier to reason about.

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
- select a dispatcher automatically
- register the actor
- return an `IAskActorRef`

Automatic dispatcher selection is based on:

```text
dispatcherIndex = actorId % dispatcherCount
```

---

### Spawn Actor (Explicit Dispatcher)

```csharp
IAskActorRef actorRef = actorSystem.SpawnOnDispatcher<SampleActor>(0);
```

Use this when an actor must run on a specific dispatcher.

---

### Spawn with Factory

```csharp
IAskActorRef actorRef = actorSystem.Spawn(() => new SampleActor());

IAskActorRef actorRef2 = actorSystem.SpawnOnDispatcher(() => new SampleActor(), 0);
```

---

### Alias Registration

```csharp
IAskActorRef actorRef = actorSystem.Spawn<SampleActor>(alias: "sample");
```

Resolve the reference later:

```csharp
if (actorSystem.TryGetActorRef("sample", out IActorRef actorRef))
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

Mailbox capacity is bounded. A message that cannot be accepted because the mailbox is full is published as a dead letter.

---

## Sending Messages

Actors communicate through messages.

`Post` sends a message without waiting for a response.

```csharp
actorRef.Post(new PingMessage());
```

Messages must implement `IActorMessage`.

```csharp
public readonly struct PingMessage : IActorMessage
{
}
```

`Post` is fire-and-forget. The caller does not receive a result from the target actor.

The runtime attempts to enqueue the message into the target actor mailbox for later processing by its dispatcher.

A message is published as a dead letter when it cannot be delivered because:

- the mailbox is full
- the target actor has stopped
- the recipient reference is invalidated
- the actor system has been disposed

---

## Dead Letters

Subscribe to `ActorSystem.OnDeadLetterDetected` to observe undeliverable messages and actor execution failures.

```csharp
actorSystem.OnDeadLetterDetected += deadLetterMessage =>
{
    // Inspect deadLetterMessage.Reason and deadLetterMessage.Message.
};
```

Dead letters are useful for operational monitoring and diagnosing delivery failures. They should not be used as a normal request/response path.

---

## Ask Request/Response

`Ask` is used when the caller needs a response from an actor.

Use `Post` for fire-and-forget messages.  
Use `Ask` only when the response is required for the next control-flow step.

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

When handling an Ask request, send the response message back to `sender`.

```csharp
sender.Post(new CreateRoomResponse()
{
    Ok = true,
    RoomNumber = request.RoomNumber
}, Self);
```

The Ask runtime internally tracks the pending operation and completes it when the response message is posted to the Ask reply reference.

`AskAsync<TResponse>` returns a `ValueTask<TResponse>`. Await the returned value once and do not store or reuse it after completion.

The response type must match the type requested by `AskAsync<TResponse>`.

If no response is received before the supplied timeout, the Ask operation completes with a timeout failure.

Use Ask for control-flow operations such as:

- room creation
- database queries
- server-side commands
- management requests

Do not use Ask for high-frequency game-loop messages.

---

## Post vs Ask

Use `Post` when the caller only needs to send a message.

Use `Ask` when the caller must wait for a response.

```text
Post
caller -> target actor mailbox
caller continues immediately
```

```text
Ask
caller -> target actor mailbox
caller awaits response
target actor -> sender.Post(response, Self)
```

In most actor flows, prefer `Post`.

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
            // Handle the message.
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

## Lifecycle

- an actor is created through `Spawn`
- incoming messages are queued in the actor mailbox
- the assigned dispatcher executes messages sequentially
- `Kill` marks the actor for termination
- the actor is removed after its kill operation is finalized

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

## Summary

- `ActorSystem` → creates, registers, routes to, and manages actors
- `ActorBase` → implements actor logic and owns actor-local state
- `IActorRef` → sends messages and controls actor lifetime
- `IAskActorRef` → sends request/response messages
- `Dispatcher` → schedules actor execution
- `IActorMessage` → defines actor messages

---

## When to Use

Use `Dignus.Actor.Core` when you need:

- actor-based concurrency
- sequential processing per entity
- message-driven architecture
- isolated actor-local state
- request/response messaging between actors
