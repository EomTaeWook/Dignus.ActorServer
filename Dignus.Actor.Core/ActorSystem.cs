// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Internals;
using Dignus.Actor.Core.Messages;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Dignus.Actor.Core
{
    public class ActorSystem
    {
        private readonly ConcurrentDictionary<long, ActorRunner> _actors = new();
        private readonly ActorDispatcher[] _dispatchers;
        private long _nextId;
        private readonly int _serverId;
        public ActorSystem(int serverId, int shardCount)
        {
            _serverId = serverId;
            _dispatchers = new ActorDispatcher[shardCount];
            for (int i = 0; i < shardCount; i++)
            {
                _dispatchers[i] = new ActorDispatcher(i);
                _dispatchers[i].Start();
            }
        }
        public IActorRef FindActorRef(long actorId)
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
            long id = Interlocked.Increment(ref _nextId);
            int dispatcherIndex = (int)(id % _dispatchers.Length);
            long actorId = ActorIdHelper.CreateId(_serverId, dispatcherIndex, id);

            IActorRef actorRef = new ActorRef(this, actorId);

            actor.Self = actorRef;

            var dispatcher = _dispatchers[dispatcherIndex];
       
            var runner = new ActorRunner(actor, dispatcher);

            if (_actors.TryAdd(actorId, runner) == false)
            {
                throw new InvalidOperationException($"Duplicate actor id.{actorId}");
            }

            return actorRef;
        }

        internal void Post(long targetId, IActorMessage message, IActorRef sender)
        {
            if (_actors.TryGetValue(targetId, out var actor) == false)
            {
                return;
            }
            actor.Enqueue(message, sender);
        }
        internal bool TryRemove(long actorId, out ActorRunner runner)
        {
            return _actors.TryRemove(actorId, out runner);
        }
    }
}
