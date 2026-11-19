# Dignus.ActorServer

[English](README.md) | 한국어

Actor별 상태 소유, 순차 메시지 처리와 TCP/TLS 전송을 제공하는 런타임·서버 라이브러리.

## 요구 사항

Actor.Core는 netstandard2.1과 .NET 8–10, 네트워크 패키지는 .NET 8–10을 대상으로 한다. 계약 패키지의 대상은 개별 프로젝트를 따른다.

## 설치

```xml
<PackageReference Include="Dignus.Actor.Core" Version="2.1.12" />
<PackageReference Include="Dignus.ActorServer" Version="3.1.12" />
```

## 빠른 시작

다음은 Actor와 메시지의 선언 예제다. 시스템 설정, Actor 생성·전송·종료는 Core 가이드를 따른다. 로컬 Actor만 사용하면 네트워크 패키지는 선택 사항이다.

```csharp
using Dignus.Actor.Abstractions;
using Dignus.Actor.Core;
using System.Threading.Tasks;

public readonly struct PingMessage : IActorMessage
{
}

public class SampleActor : ActorBase
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

## 사용법

가변 상태는 소유 Actor의 실행 문맥에서 관리한다. Ask, 디스패처 전환, 세션 Actor, 프로토콜과 Send·SendAsync 차이는 가이드를 따른다.

## 문서

- [구성과 전송 동작 · 영어](docs/Usage.md)
- [Actor 계약 · 영어](publish/Dignus.Actor.Abstractions.md)
- [Actor 런타임 API · 영어](publish/Dignus.Actor.Core.md)
- [네트워크 API · 영어](publish/Dignus.Actor.Network.md)

## 라이선스

MIT 라이선스. [LICENSE](LICENSE)를 참조한다.
