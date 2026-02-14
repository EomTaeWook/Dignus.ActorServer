// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Messages;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Dignus.Actor.Core
{
    public class ActorSystem
    {
        private readonly ConcurrentDictionary<int, ActorRunner> _actors = new();
        private readonly ActorDispatcher[] _dispatchers;
        private int _nextId;
        public ActorSystem(int shardCount)
        {
            _dispatchers = new ActorDispatcher[shardCount];
            for (int i = 0; i < shardCount; i++)
            {
                _dispatchers[i] = new ActorDispatcher(i);
                _dispatchers[i].Start();
            }
        }
        public IActorRef Spawn<TActor>() where TActor : ActorBase, new()
        {
            return SpawnInternal(new TActor());
        }
        public IActorRef Spawn<TActor>(Func<TActor> factory) where TActor : ActorBase
        {
            return SpawnInternal(factory());
        }

        private ActorRef SpawnInternal<TActor>(TActor actor) where TActor : ActorBase
        {
            int id = Interlocked.Increment(ref _nextId);

            int dispatcherIndex = id % _dispatchers.Length;
            var dispatcher = _dispatchers[dispatcherIndex];

            ActorRef actorRef = new(this, id);

            actor.Bind(dispatcher, actorRef);

            var runner = new ActorRunner(actor, dispatcher, FinalizeKill);

            if (_actors.TryAdd(id, runner) == false)
            {
                throw new InvalidOperationException($"Duplicate actor id.{id}");
            }

            return actorRef;
        }

        internal void Post(int actorId, IActorMessage message, IActorRef sender)
        {
            if (_actors.TryGetValue(actorId, out var actor) == false)
            {
                return;
            }

            actor.Enqueue(message, sender);
        }
        internal void Kill(int actorId)
        {
            if (_actors.TryGetValue(actorId, out var actor))
            {
                actor.Kill();
            }
        }
        internal void FinalizeKill(int actorId)
        {
            _actors.TryRemove(actorId, out _);
        }
    }
}
