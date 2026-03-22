# Dignus.Actor.Abstractions

Minimal contracts for the Dignus Actor framework.

---

## Overview

`Dignus.Actor.Abstractions` contains the core interfaces shared across the actor ecosystem.

This package is designed to be lightweight and dependency-free, allowing it to be used in any environment, including Unity.

---

## Purpose

This package exists to:

- Define shared contracts between components
- Avoid referencing the full actor runtime
- Enable protocol and model libraries to integrate with the actor system

---

## Included

### IActorMessage

Marker interface for actor messages.

- Represents a message that can be sent between actors
- Has no behavior
- Intended for type safety and consistency

---

## Design

- No runtime logic
- No dependencies on Actor.Core
- Safe to use in shared model projects

---

## When to Use

Use this package when:

- Defining shared message contracts
- Building protocol/model libraries
- Integrating with the actor system without referencing the runtime

---

## Summary

`Dignus.Actor.Abstractions` provides the minimal building blocks required to define actor messages without depending on the full runtime.