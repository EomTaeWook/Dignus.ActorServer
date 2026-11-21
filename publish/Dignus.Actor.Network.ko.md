# Dignus.ActorServer

[English](Dignus.Actor.Network.md) | 한국어

[한국어 개요](../README.ko.md) | [Actor 런타임 API](Dignus.Actor.Core.ko.md)

Actor.Core 실행 모델과 TCP·TLS 네트워크, 프로토콜 기반 디스패치를 결합한다. 패킷을 메시지로 역직렬화해 세션 Actor에서 순차 처리한다. 예제의 계정 모델·프로토콜·상태 제어기는 소비 프로젝트가 제공하는 구성 예시다.

## 프로토콜 등록

ProtocolBodyTypeMapper에 프로토콜과 메시지 타입을 등록한다. enum 이름과 본문 타입 이름이 같으면 단일·복수 어셈블리에서 자동 등록할 수 있다.

```csharp
private static void RegisterProtocol(IServiceProvider serviceProvider)
{
    var mapper = serviceProvider.GetService<ProtocolBodyTypeMapper>();

    mapper.AddMapping<CreateAccountReq>(CLSProtocol.CreateAccountReq);
    mapper.AddMapping<LoginReq>(CLSProtocol.LoginReq);
}
```

```csharp
private static void RegisterProtocol(IServiceProvider serviceProvider)
{
    var mapper = serviceProvider.GetService<ProtocolBodyTypeMapper>();

    mapper.RegisterByProtocolName<CLSProtocol>(typeof(CreateAccountReq).Assembly);
}
```

```csharp
private static void RegisterProtocol(IServiceProvider serviceProvider, IEnumerable<Assembly> assemblies)
{
    var mapper = serviceProvider.GetService<ProtocolBodyTypeMapper>();

    mapper.RegisterByProtocolName<CLSProtocol>(assemblies);
}
```

```csharp
public enum CLSProtocol
{
    CreateAccountReq = 1001,
    LoginReq = 1002
}

public sealed class CreateAccountReq : IActorMessage
{
}

public sealed class LoginReq : IActorMessage
{
}
```

## 서버 설정

프로토콜 등록 → Decoder·Serializer 설정 → ActorSystem 생성 → 세션 Actor·서버 구현 → Start 순서로 구성한다.

```csharp
RegisterProtocol(serviceProvider);

var serverOptions = ServerOptions.Builder()
    .UseDecoder(new PacketFramer(serviceProvider))
    .UseSerializer(new MessageSerializer())
    .Build();

var actorSystem = new ActorSystem();

var lobby = new LobbyServer(actorSystem, serverOptions, serviceProvider);
lobby.Start(30000);
```

## 패킷 Decoder

길이 헤더로 수신 패킷을 분리하고 프로토콜을 조회해 본문을 메시지로 역직렬화한다.

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

## Serializer

수신 바이트와 송신 메시지의 표현을 연결한다.

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

## 세션 Actor

세션별 상태와 업무 메시지를 처리하고 NetworkSession으로 응답을 보낸다.

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

## 서버 구현

TcpServerBase에서 세션 Actor를 생성하고 dead letter를 처리한다. TLS는 TlsServerBase를 사용하며 전송 계층을 제외한 구성 방식은 같다.

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

## 실행 문맥과 프로토콜

수신 → 프레이밍 → 프로토콜·본문 역직렬화 → Actor 메일박스 → OnReceive → 응답 순서다. Actor별 실행은 단일 디스패처 문맥에서 직렬화한다. 기본 방식은 핸들러 바인딩 없이 메시지를 Actor로 직접 전달한다. 인증·검사·로깅·필터·복합 제어가 필요하면 미들웨어와 핸들러 파이프라인을 사용한다.

## Send와 SendAsync

내부 송신 경로와 순서 보존 방식은 같다. 세션 Actor에서 Send를 호출하면 디스패처 스레드에서 송신 루프를 실행한다. SendAsync는 Task.Run으로 ThreadPool에 예약하므로 추가 스케줄링·할당·GC 비용이 생길 수 있다. 실행할 문맥에 맞춰 선택한다.

## 성능 측정

원문 환경: Intel Core i5-12400F, 6코어·12스레드, 최대 4.40GHz, RAM 32GB, .NET 10 Release x64. 127.0.0.1:5000에서 32바이트 메시지를 10초 동안 측정했다. TCP 왕복은 클라이언트 1개와 진행 중 메시지 1000개, fan-out은 100개 클라이언트다. TLS도 각각 1개·100개 클라이언트 조건이다.

아래는 TCP 왕복 → TCP fan-out → TLS 왕복 → TLS fan-out 순서의 기존 결과이며 새로 측정한 값이 아니다.

```text
Total Time: 10.002 seconds
Total Client: 1
Total Bytes: 4,364,075,744
Total Data: 4.06 GiB
Total Message: 136,377,367
Data Throughput: 416.11 MiB/s
Message Throughput: 13,634,960 msg/s
```

```text
Total Time: 10.008 seconds
Total Client: 100
Total Bytes: 4,909,594,848
Total Data: 4.57 GiB
Total Message: 153,424,839
Data Throughput: 467.83 MiB/s
Message Throughput: 15,329,728 msg/s
```

```text
Total Time: 10.011 seconds
Total Client: 1
Total Bytes: 3,531,230,304
Total Data: 3.29 GiB
Total Message: 110,350,947
Data Throughput: 336.41 MiB/s
Message Throughput: 11,023,486 msg/s
```

```text
Total Time: 10.052 seconds
Total Client: 100
Total Bytes: 4,852,865,632
Total Data: 4.52 GiB
Total Message: 151,652,051
Data Throughput: 460.42 MiB/s
Message Throughput: 15,087,042 msg/s
```

측정은 Decode → Actor → Encode → Send까지 포함하며 Actor 실행 자체에는 ThreadPool을 사용하지 않는다. 서버는 네트워크 진입점, Decoder는 패킷 해석, Serializer는 출력 버퍼, Mapper는 프로토콜 해석, Actor는 업무 상태를 담당한다.
