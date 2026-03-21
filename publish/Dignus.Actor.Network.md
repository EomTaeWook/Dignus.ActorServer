# Dignus.ActorServer

[![NuGet](https://img.shields.io/nuget/v/Dignus.ActorServer.svg)](https://www.nuget.org/packages/Dignus.ActorServer/)

High-performance Actor-based network server framework.

---

## Performance

### Benchmark Environment
- CPU: Intel Core i5-12400F (12th Gen)
- Cores / Threads: 6 / 12
- Max Turbo Frequency: 4.40 GHz
- Memory: 32 GB
- Architecture: x64
- Operating System: Windows 64-bit
- Runtime: .NET 10 (Release x64)

### Round-Trip Benchmark (Plain TCP)

This benchmark measures full round-trip throughput:

Client send -> Server-side processing -> Response return

<p align="center">
  <img src="Benchmark/Result/tcp-round-trip.png" width="600" />
</p>

### Test Conditions

- Server address: 127.0.0.1
- Server port: 5000
- Protocol: Plain TCP (no TLS)
- Working clients: 1
- In-flight messages per client: 1000
- Message size: 32 bytes
- Benchmark duration: 10 seconds

### Result

```
Total Time: 10.008 seconds
Total Client: 1
Total Bytes: 3,269,163,360
Total Data: 3.04 GiB
Total Message: 102,161,355
Data Throughput: 311.53 MiB/s
Message Throughput: 10,208,278 msg/s
```

---

### TCP Fan-out Benchmark (100 clients)

Send pattern: Server broadcasts identical payload to all connected clients

<p align="center">
  <img src="Benchmark/Result/tcp-fan-out-100.png" width="600" />
</p>

### Test Conditions

- Server address: 127.0.0.1
- Server port: 5000
- Protocol: Plain TCP (no TLS)
- Working clients: 100
- Message size: 32 bytes
- Benchmark duration: 10 seconds

### Result

```
Total Time: 10.008 seconds
Total Client: 100
Total Bytes: 4,909,594,848
Total Data: 4.57 GiB
Total Message: 153,424,839
Data Throughput: 467.83 MiB/s
Message Throughput: 15,329,728 msg/s
```

---

### TLS Round-Trip Benchmark

This benchmark measures full round-trip throughput:

Client send -> Server-side processing -> Response return

<p align="center">
  <img src="Benchmark/Result/tls-round-trip.png" width="600" />
</p>

### Test Conditions

- Server address: 127.0.0.1
- Server port: 5000
- Protocol: TLS over TCP
- Working clients: 1
- In-flight messages per client: 1000
- Message size: 32 bytes
- Benchmark duration: 10 seconds

### Result

```
Total Time: 10.002 seconds
Total Client: 1
Total Bytes: 2,482,299,424
Total Data: 2.31 GiB
Total Message: 77,571,857
Data Throughput: 236.68 MiB/s
Message Throughput: 7,755,636 msg/s
```

---

### Tls Fan-out Benchmark (100 clients)

Send pattern: Server broadcasts identical payload to all connected clients

<p align="center">
  <img src="Benchmark/Result/tls-fan-out-100.png" width="600" />
</p>

### Test Conditions

- Server address: 127.0.0.1
- Server port: 5000
- Protocol: TLS over TCP
- Working clients: 100
- Message size: 32 bytes
- Benchmark duration: 10 seconds

### Result

```
Total Time: 10.052 seconds
Total Client: 100
Total Bytes: 4,852,865,632
Total Data: 4.52 GiB
Total Message: 151,652,051
Data Throughput: 460.42 MiB/s
Message Throughput: 15,087,042 msg/s
```

---

### Performance Highlights

- Over 10 million round-trip messages per second
- Sustained throughput above 300 MiB/sec
- Full end-to-end measurement (decode -> actor execution -> encode -> send)
- Execution confined to dedicated dispatcher threads
- No ThreadPool scheduling for actor logic

---

## Design Goals

- Strict separation of session logic and network I/O
- Single-threaded execution guarantee per actor
- Partition-based dispatcher scheduling
- Async/await support with dispatcher-context enforcement
- Message-driven concurrency model

---

## Core Architecture

### ActorSystem

`ActorSystem` manages:

- Multiple `ActorDispatcher` instances
- Actor lifecycle
- Partition-based distribution

Actors are distributed using:

```
dispatcherIndex = actorId % dispatcherCount
```

Each actor executes through an `ActorRunner`.

---

### ActorDispatcher

Each dispatcher:

- Owns a dedicated worker thread
- Maintains a lock-free scheduling queue
- Uses SemaphoreSlim for wake-up signaling
- Enforces dispatcher-thread execution context

Guarantees:

- An actor always executes on the same thread
- Async continuations resume on the dispatcher thread
- No ThreadPool execution for actor logic

---

### ActorRunner

Execution engine of an actor.

Responsibilities:

- Mailbox processing
- Lifecycle management
- ValueTask-based async handling
- Continuation rescheduling

Execution model:

1. Dequeue message
2. Execute OnReceive
3. If async incomplete -> schedule continuation
4. Resume on dispatcher thread

This guarantees logical single-threaded execution per actor.

---

## Network Layer

```
TcpServerBase / TlsServerBase
    |
    v
ActorPacketProcessor
    |
    v
SessionActor
    |
    v
NetworkSession
```

---

## Concurrency Model

- Single-threaded execution per actor
- Dedicated dispatcher threads
- No shared mutable state across actors
- Message-passing communication model
- Lock-free mailbox scheduling

---

## Lifecycle Model

Kill flow:

1. sessionRef.Kill()
2. ActorRunner transitions to killing state
3. Finalization executed on dispatcher thread
4. Mailbox cleared
5. Actor removed from ActorSystem
6. TransportActor disposes underlying session

## Protocol Pipeline Flow

`Dignus.ActorServer` protocol pipeline works in two phases:

1. **Registration phase**
2. **Runtime execution phase**

---

## 1. Registration phase

At startup, protocol handlers are scanned and bound to the pipeline.

    ActorProtocolPipeline.Register<TProtocol>()
        ↓
    Protocol method scan
        ↓
    Protocol name binding
        ↓
    Body type extraction
        ↓
    Middleware registration
        ↓
    Compiled protocol invoker build

During registration the framework performs the following steps:

- Protocol handler methods are discovered
- Protocol names are mapped to handler methods
- Handler parameter body types are extracted and cached
- Middleware is registered per handler method
- Dispatch delegates are compiled for runtime execution

Example registration:

```csharp
ActorProtocolPipeline<ClientPipelineContext>.Register<CLSProtocol>((method, pipeline) =>
{
    var filters = method.GetCustomAttributes<ActionAttribute>();
    var orderedFilters = filters.OrderBy(r => r.Order).ToList();

    var middleware = new ProtocolActionMiddleware(orderedFilters);
    pipeline.Use(middleware);
});
```

---

## 2. Runtime execution phase

When a TCP packet arrives, the protocol pipeline executes the following flow.

    TCP packet received
        ↓
    PacketFramer.Deserialize
        ↓
    Protocol number extracted
        ↓
    Protocol validation
        ↓
    Body type resolved from registered protocol metadata
        ↓
    JSON body deserialization
        ↓
    Actor message creation
        ↓
    Actor mailbox post
        ↓
    PlayerActor.OnReceive
        ↓
    Pipeline execution
        ↓
    Middleware execution
        ↓
    Protocol handler invocation
        ↓
    Actor state processing

---

## Decode stage (Network layer)

Incoming packets are decoded and transformed into actor messages.

```csharp
public IActorMessage Deserialize(ReadOnlySpan<byte> packet)
{
    int protocol = BitConverter.ToUInt16(packet[..ProtocolSize]);

    if (ActorProtocolPipeline<ClientPipelineContext>.ValidateProtocol(protocol) == false)
    {
        LogHelper.Error($"not found protocol : {protocol}");
        return null;
    }

    var bodyString = Encoding.UTF8.GetString(packet[ProtocolSize..]);

    var bodyType = ActorProtocolPipeline<ClientPipelineContext>.GetBodyType(protocol);

    var bodyPacketObject = JsonSerializer.Deserialize(bodyString, bodyType);

    async Task lambdaMessage(PlayerActor actor)
    {
        var context = new ClientPipelineContext()
        {
            Body = bodyPacketObject,
            Handler = clientPacketHandler,
            Protocol = protocol,
            State = actor
        };

        await ActorProtocolPipeline<ClientPipelineContext>.ExecuteAsync(ref context);
    }

    return new InBoundLambdaMessage(lambdaMessage);
}
```

---

## Actor execution stage

The actor receives the decoded message and executes the protocol pipeline.

```csharp
protected override async ValueTask OnReceive(IActorMessage message, IActorRef sender)
{
    if (message is InBoundLambdaMessage inBoundLambdaMessage)
    {
        inBoundLambdaMessage.Invoke(this);
    }
}
```

---

## Final handler execution

The pipeline eventually invokes the protocol handler.

```csharp
[ProtocolName("Login")]
public async Task Process(PlayerActor actor, Login packet)
{
    var currentState = actor.GetCurrentState();
    await currentState.HandlePacketAsync(packet);
}
```

---

## Execution Summary

    Register
    → Protocol binding
    → Body type extraction
    → Middleware registration

    Receive packet
    → Decode packet
    → Resolve BodyType
    → Deserialize body
    → Create actor message
    → Post to actor mailbox
    → Execute pipeline
    → Run middleware
    → Invoke handler
    → Actor state logic

---

## Key Characteristics

- Packet decoding happens before actor execution
- Actor logic remains lightweight
- Middleware executes inside the actor context
- No shared state across actors