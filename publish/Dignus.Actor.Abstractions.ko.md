# Dignus.Actor.Abstractions

[English](Dignus.Actor.Abstractions.md) | 한국어

[한국어 개요](../README.ko.md)

Actor 생태계가 공유하는 최소 계약이다. 전체 Actor 런타임을 참조하지 않고 프로토콜·공유 모델에서 사용하도록 가볍게 구성한다. Unity 등에서도 사용할 수 있다.

## IActorMessage

Actor 간 전송할 메시지를 나타내는 마커 인터페이스다. 별도 동작을 요구하지 않으며 소비 프로젝트가 데이터를 정의한다. 타입 안전성과 메시지 계약의 일관성을 제공한다.

## 의존성 경계

런타임 실행 로직과 Actor.Core 의존성을 포함하지 않는다. 공유 메시지 계약이나 프로토콜·모델 라이브러리에 추가하고 실제 실행이 필요할 때 [Actor.Core](Dignus.Actor.Core.ko.md)를 사용한다.
