# Dignus.Actor.Core

[English](Dignus.Actor.Core.md) | 한국어

[한국어 개요](../README.ko.md) | [네트워크 API](Dignus.Actor.Network.ko.md)

Actor 실행·메시징·스케줄링·수명을 담당한다. 네트워크는 별도 Dignus.ActorServer 패키지다. Actor는 전용 디스패처에서 메시지를 순차 처리하고 가변 상태를 소유한다.

## 주요 구성 요소

- ActorSystem: 생성·등록·메일박스 라우팅·디스패처 배정·별칭 조회·수명·dead letter 발행.
- ActorBase: 수신 처리와 로컬 상태. Self는 IAskActorRef이며 VerifyContext로 실행 문맥을 확인한다.
- IActorMessage: 동작을 요구하지 않는 메시지 데이터 계약.
- IActorRef: Actor 인스턴스를 노출하지 않고 Post·Kill을 제공한다.
- IAskActorRef: 응답을 기다리는 Ask를 제공하는 참조. 저빈도 제어 흐름에 사용한다.
- Dispatcher: 전용 작업 스레드에서 스케줄링하고 같은 Actor의 동시 실행을 막는다.

디스패처 동기화 문맥을 캡처한 continuation은 해당 스레드에서 실행된다. ConfigureAwait(false)처럼 문맥을 피한 continuation에는 같은 보장이 없다.

## 생성과 등록

기본 디스패처 수는 Environment.ProcessorCount다. 자동 배정은 actorId % dispatcherCount이며 명시적 배정과 팩토리 생성도 지원한다. 별칭으로 참조를 조회할 수 있다. 메일박스 용량이 차서 수용하지 못한 메시지는 dead letter가 된다.

```csharp
var actorSystem = new ActorSystem();
```

```csharp
IAskActorRef actorRef = actorSystem.Spawn<SampleActor>();
```

```text
dispatcherIndex = actorId % dispatcherCount
```

```csharp
IAskActorRef actorRef = actorSystem.SpawnOnDispatcher<SampleActor>(0);
```

```csharp
IAskActorRef actorRef = actorSystem.Spawn(() => new SampleActor());

IAskActorRef actorRef2 = actorSystem.SpawnOnDispatcher(() => new SampleActor(), 0);
```

```csharp
IAskActorRef actorRef = actorSystem.Spawn<SampleActor>(alias: "sample");
```

```csharp
if (actorSystem.TryGetActorRef("sample", out IActorRef actorRef))
{
}
```

```csharp
IAskActorRef actorRef = actorSystem.Spawn<SampleActor>(
    alias: "sample",
    mailboxCapacity: 2048);
```

## 메시지 전송

Post는 응답을 기다리지 않는다. 런타임이 대상 메일박스에 넣고 디스패처가 나중에 처리한다. 메시지는 IActorMessage를 구현한다.

```csharp
actorRef.Post(new PingMessage());
```

```csharp
public readonly struct PingMessage : IActorMessage
{
}
```

메일박스 가득 참, Actor 중단, 참조 무효화, 시스템 해제로 전달하지 못하면 dead letter를 발행한다.

## Dead letter

OnDeadLetterDetected로 전달 실패와 실행 예외를 관찰한다. 운영 진단에 사용하며 정상 요청·응답 경로로 사용하지 않는다.

```csharp
actorSystem.OnDeadLetterDetected += deadLetterMessage =>
{
    // Inspect deadLetterMessage.Reason and deadLetterMessage.Message.
};
```

## Ask 요청·응답

다음 단계에 응답이 필요한 경우에만 사용하고 고빈도 게임 루프에는 Post를 우선한다. 요청·응답에 별도 request id를 노출할 필요 없이 IActorMessage를 구현한다. 수신자는 sender.Post(response, Self)로 회신한다.

```csharp
CreateRoomResponse response = await actorRef.AskAsync<CreateRoomResponse>(
    new CreateRoomRequest(),
    3000);
```

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
sender.Post(new CreateRoomResponse()
{
    Ok = true,
    RoomNumber = request.RoomNumber
}, Self);
```

런타임이 대기 작업을 추적하고 회신 참조에 응답이 도착하면 완료한다. AskAsync의 ValueTask는 한 번만 await하며 완료 후 저장·재사용하지 않는다. 응답 타입이 요청한 타입과 같아야 하고 제한 시간 내 응답이 없으면 타임아웃 실패다. 방 생성·DB 조회·서버 명령·관리 요청에 사용할 수 있다.

## Actor 예제

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

## Ask 예제

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

## 수명

Spawn으로 생성하고 메일박스에 메시지를 넣는다. 배정된 디스패처가 순차 실행하며 Kill은 종료 대상으로 표시한다. 종료 처리가 확정되면 Actor를 제거한다.

## 벤치마크

아래는 원문의 로컬 프로세스 내 ping-pong 측정이다. 네트워크·직렬화·DB·로깅·게임 로직은 포함하지 않는다. 메시지마다 전역 동기화를 하지 않으며 CPU 스케줄링·백그라운드 부하·전원 모드·GC·워밍업에 따라 달라진다.

```text
CPU: Intel Core i5-12400F
RAM: 32 GB
OS: Windows x64
```

```text
Actor Pair Count: 348
Actual Actor Count: 696
Pipeline Size Per Pair: 1,000
Benchmark Duration: 10 seconds
Counter: per-actor local counter, summed after completion
```

```text
Processed Messages: 2,907,908,768
Elapsed: 10.475 sec
Throughput: 277,599,493 msg/s
```

```text
Throughput: around 250M ~ 270M msg/s
```

[전체 구성과 전송 동작](../docs/Usage.ko.md)도 참조한다.
