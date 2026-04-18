# Dignus.ActorServer

[![NuGet](https://img.shields.io/nuget/v/Dignus.ActorServer.svg)](https://www.nuget.org/packages/Dignus.ActorServer/)

High-performance actor-based network server framework.

---

## Overview

`Dignus.ActorServer` combines:

- Actor-based execution (`Dignus.Actor.Core`)
- High-performance networking (TCP / TLS)
- Protocol-based message dispatch

Incoming network packets are decoded into actor messages and processed sequentially.

---

## Quick Start

A minimal server consists of:

1. Register protocol mappings  
2. Configure server options (decoder / serializer)  
3. Create actor system  
4. Implement session actor  
5. Implement server  
6. Start server  

---

## 1. Protocol Registration

```csharp
private static void RegisterProtocol(IServiceProvider serviceProvider)
{
    var mapper = serviceProvider.GetService<ProtocolBodyTypeMapper>();

    mapper.AddMapping<CreateAccountReq>(CLSProtocol.CreateAccountReq);
    mapper.AddMapping<LoginReq>(CLSProtocol.LoginReq);
}
```

---

## 2. Server Setup

```csharp
RegisterProtocol(serviceProvider);

var serverOptions = ServerOptions.Builder()
    .UseDecoder(new PacketFramer(serviceProvider))
    .UseSerializer(new MessageSerializer())
    .Build();

var actorSystem = serviceProvider.GetService<ActorSystem>();

var lobby = new LobbyServer(actorSystem, serverOptions, serviceProvider);
lobby.Start(30000);
```

---

## 3. Packet Decoder

```csharp
internal class PacketFramer(IServiceProvider serviceProvider) : IActorMessageDecoder
{
    protected const int PacketSize = sizeof(int);
    protected const int ProtocolSize = sizeof(ushort);

    private readonly ProtocolBodyTypeMapper _bodyTypeMapper =
        serviceProvider.GetService<ProtocolBodyTypeMapper>();

    public IActorMessage Deserialize(ReadOnlySpan<byte> packet)
    {
        int protocol = BitConverter.ToUInt16(packet[..PacketSize]);

        if (_bodyTypeMapper.ContainsProtocol(protocol) == false)
            return null;

        var bodyString = Encoding.UTF8.GetString(packet[ProtocolSize..]);
        var bodyType = _bodyTypeMapper.GetBodyType(protocol);

        return (IActorMessage)JsonSerializer.Deserialize(bodyString, bodyType);
    }

    public bool TryFrame(ISession session, ArrayQueue<byte> buffer,
        out ArraySegment<byte> packet,
        out int consumedBytes)
    {
        packet = default;
        consumedBytes = 0;

        if (buffer.Count < PacketSize)
            return false;

        var packetSize = BitConverter.ToInt32(buffer.Peek(PacketSize));

        if (buffer.Count < packetSize + PacketSize)
            return false;

        consumedBytes = BitConverter.ToInt32(buffer.Read(PacketSize));
        return buffer.TrySlice(out packet, consumedBytes);
    }
}
```

---

## 4. Serializer

```csharp
internal class MessageSerializer : IActorMessageSerializer
{
    public IActorMessage Deserialize(ArraySegment<byte> bytes)
    {
        return new BinaryMessage(bytes);
    }

    public ArraySegment<byte> MakeSendBuffer(INetworkActorMessage packet)
    {
        return packet is BinaryMessage msg ? msg.Data : default;
    }

    public ArraySegment<byte> MakeSendBuffer(IPacket packet)
    {
        return default;
    }
}
```

---

## 5. Session Actor

```csharp
public class PlayerActor : SessionActorBase
{
    private readonly StateController _stateController;
    private Player _player;

    public PlayerActor(IServiceProvider serviceProvider)
    {
        _stateController = new StateController(this, serviceProvider);
    }

    protected override ValueTask OnReceive(IActorMessage message, IActorRef sender)
    {
        if (message is KickUserMessage kick)
        {
            Send(PacketFactory.MakePacket(LSCProtocol.ServerNotify, 0,
                new ServerNotify { ErrorCode = kick.Reason }));

            ChangeState(PlayerStateType.Disconnect);
        }
        else
        {
            return _stateController.HandleMessageAsync(message, sender);
        }

        return ValueTask.CompletedTask;
    }

    public void Send(INetworkActorMessage message)
    {
        NetworkSession.SendAsync(message);
    }

    public void Send(IPacket packet)
    {
        NetworkSession.SendAsync(packet);
    }
}
```

---

## 6. Server Implementation

```csharp
internal class LobbyServer(
    ActorSystem actorSystem,
    ServerOptions serverOptions,
    IServiceProvider serviceProvider)
    : TcpServerBase<PlayerActor>(actorSystem, serverOptions)
{
    protected override PlayerActor CreateSessionActor()
    {
        var actor = new PlayerActor(serviceProvider);
        actor.ChangeState(PlayerStateType.Initial);
        return actor;
    }

    protected override void OnDeadLetterMessage(DeadLetterMessage deadLetterMessage)
    {
        LogHelper.Error($"{deadLetterMessage.Reason}");

        if (deadLetterMessage.Reason == DeadLetterReason.ExecutionException)
        {
            var exceptionMessage = deadLetterMessage.Message as ActorExceptionMessage;
            LogHelper.Error(exceptionMessage.Exception);
        }
    }
}
```

---

## Execution Flow

```
TCP Packet
    ↓
Frame
    ↓
Deserialize (protocol → message)
    ↓
Actor Mailbox
    ↓
Actor.OnReceive
    ↓
Send Response
```

---

## Performance

### Benchmark Environment

- CPU: Intel Core i5-12400F (12th Gen)
- Cores / Threads: 6 / 12
- Max Turbo Frequency: 4.40 GHz
- Memory: 32 GB
- Runtime: .NET 10 (Release x64)

---

### Test Conditions

- Server address: 127.0.0.1
- Server port: 5000
- Protocol: Plain TCP (no TLS)
- Working clients: 1
- In-flight messages per client: 1000
- Message size: 32 bytes
- Benchmark duration: 10 seconds

### Round-Trip (TCP)

```text
Total Time: 10.002 seconds
Total Client: 1
Total Bytes: 4,364,075,744
Total Data: 4.06 GiB
Total Message: 136,377,367
Data Throughput: 416.11 MiB/s
Message Throughput: 13,634,960 msg/s
```

---

### Test Conditions

- Server address: 127.0.0.1
- Server port: 5000
- Protocol: Plain TCP (no TLS)
- Working clients: 100
- Message size: 32 bytes
- Benchmark duration: 10 seconds

### Fan-out (100 clients)

```text
Total Time: 10.008 seconds
Total Client: 100
Total Bytes: 4,909,594,848
Total Data: 4.57 GiB
Total Message: 153,424,839
Data Throughput: 467.83 MiB/s
Message Throughput: 15,329,728 msg/s
```

---

### Test Conditions

- Server address: 127.0.0.1
- Server port: 5000
- Protocol: TLS over TCP
- Working clients: 1
- In-flight messages per client: 1000
- Message size: 32 bytes
- Benchmark duration: 10 seconds

### TLS Round-Trip

```text
Total Time: 10.011 seconds
Total Client: 1
Total Bytes: 3,531,230,304
Total Data: 3.29 GiB
Total Message: 110,350,947
Data Throughput: 336.41 MiB/s
Message Throughput: 11,023,486 msg/s
```

---

### Test Conditions

- Server address: 127.0.0.1
- Server port: 5000
- Protocol: TLS over TCP
- Working clients: 100
- Message size: 32 bytes
- Benchmark duration: 10 seconds

### TLS Fan-out (100 clients)

```text
Total Time: 10.052 seconds
Total Client: 100
Total Bytes: 4,852,865,632
Total Data: 4.52 GiB
Total Message: 151,652,051
Data Throughput: 460.42 MiB/s
Message Throughput: 15,087,042 msg/s
```

---

### Highlights

- 10M+ messages/sec throughput
- 300–450 MiB/s sustained throughput
- full end-to-end measurement (decode → actor → encode → send)
- no ThreadPool usage for actor execution

---

## Architecture Overview

```
TcpServerBase / TlsServerBase
        ↓
ActorPacketProcessor
        ↓
SessionActor
        ↓
Actor
```

---

## Concurrency Model

- single-threaded execution per actor
- dispatcher-based scheduling
- no shared mutable state
- message-driven processing

---

## Protocol Model (Default)

```
Protocol → BodyType → Deserialize → Actor
```

- no handler binding
- minimal overhead
- direct actor execution

---

## Protocol Pipeline (Advanced)

```
Protocol → Middleware → Handler → Actor
```

Use when:

- authentication required
- validation needed
- logging / filtering required
- complex execution flow needed

---

## TCP / TLS Support

- TCP → `TcpServerBase<TActor>`
- TLS → `TlsServerBase<TActor>`

Usage is identical except transport layer.

---

## Summary

- Actor → business logic  
- ActorSystem → execution  
- Server → network entry point  
- Decoder → packet parsing  
- Serializer → packet writing  
- Mapper → protocol resolution  

---