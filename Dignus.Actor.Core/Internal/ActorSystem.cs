// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Actors;
using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Messages;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Dignus.Actor.Core.Internal
{
    public class ActorSystem
    {
        private readonly ConcurrentDictionary<long, ActorRunner> _actors = new ConcurrentDictionary<long, ActorRunner>();
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
        public IActorRef Spawn<TActor>() where TActor : ActorBase, new()
        {
            long id = Interlocked.Increment(ref _nextId);
            int dispatcherIndex = (int)(id % _dispatchers.Length);
            long actorId = ActorIdHelper.CreateId(_serverId, dispatcherIndex, id);

            var actorRef = new ActorRef(this, actorId);
            var actor = new TActor()
            {
                Self = actorRef
            };

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
