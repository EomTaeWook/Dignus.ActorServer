# Dignus Actor Framework

[English](Usage.md) | 한국어

[한국어 개요](../README.ko.md)

메시지 기반 시스템을 위한 Actor 런타임과 네트워크 계층이다. Actor별 순차 실행과 가변 상태 격리를 기본으로 한다.

## 패키지 구조

애플리케이션 → Dignus.ActorServer(세션·프로토콜·TCP/TLS) → Dignus.Actor.Core(실행·디스패처·메시징) → Dignus.Actor.Abstractions(메시지 계약) 순서로 구성한다.

Abstractions는 IActorMessage 계약, Core는 Actor 수명·참조·Ask·dead letter, ActorServer는 네트워크와 선택적 프로토콜 파이프라인을 제공한다. 로컬 Actor는 네트워크 없이 사용할 수 있다.

## 메시지 실행

네트워크 바이트를 패킷으로 분리하고 메시지로 역직렬화한 뒤 Actor 메일박스에 넣는다. 디스패처가 Actor.OnReceive를 실행한다. 기본 직접 메시지 모델에는 핸들러·미들웨어가 필수가 아니다.

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

## 전송 방식

Send와 SendAsync는 같은 내부 송신 경로를 사용한다. Actor 문맥에서 Send는 디스패처 스레드가 송신 루프를 실행하고 SendAsync는 Task.Run으로 ThreadPool에 넘긴다. 후자는 실행 부담을 옮기는 대신 추가 스케줄링·할당·GC 비용이 생길 수 있다.

## 사용 가이드

- [공유 메시지 계약](../publish/Dignus.Actor.Abstractions.ko.md)
- [Actor 생성·메일박스·Ask·수명·벤치마크](../publish/Dignus.Actor.Core.ko.md)
- [TCP/TLS·프로토콜·세션 Actor·네트워크 벤치마크](../publish/Dignus.Actor.Network.ko.md)
