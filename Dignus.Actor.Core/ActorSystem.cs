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
        public IActorRef FindActorRef(int actorId)
        {
            if (_actors.TryGetValue(actorId, out var actor) == true)
            {
                return actor.GetActorRef();
            }
            return null;
        }
        public IActorRef Spawn<TActor>() where TActor : ActorBase, new()
        {
            return SpawnInternal(new TActor());
        }
        public IActorRef Spawn<TActor>(Func<TActor> factory) where TActor : ActorBase
        {
            return SpawnInternal(factory());
        }

        private IActorRef SpawnInternal<TActor>(TActor actor) where TActor : ActorBase
        {
            int id = Interlocked.Increment(ref _nextId);

            IActorRef actorRef = new ActorRef(this, id);

            actor.Self = actorRef;

            int dispatcherIndex = id % _dispatchers.Length;
            var dispatcher = _dispatchers[dispatcherIndex];
       
            var runner = new ActorRunner(actor, dispatcher);

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
        internal bool TryRemove(int actorId, out ActorRunner runner)
        {
            return _actors.TryRemove(actorId, out runner);
        }
    }
}
