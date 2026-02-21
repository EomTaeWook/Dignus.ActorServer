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
    public class ActorSystem : IActorRefProvider
    {
        public event Action<DeadLetterMessage> OnDeadLetterDetected;

        const int DefaultMailboxCapacity = 1024;

        private readonly ConcurrentDictionary<int, ActorRunner> _actorRunners = new();
        private readonly ConcurrentDictionary<string, int> _aliasToId = new();
        private readonly ActorDispatcher[] _dispatchers;
        private int _nextActorId;
        public ActorSystem(int dispatcherThreadCount)
        {
            _dispatchers = new ActorDispatcher[dispatcherThreadCount];
            for (int i = 0; i < dispatcherThreadCount; i++)
            {
                _dispatchers[i] = new ActorDispatcher(i);
                _dispatchers[i].Start();
            }
        }
        public IActorRef Spawn<TActor>(string alias = null,
            int mailboxCapacity = DefaultMailboxCapacity
            ) where TActor : ActorBase, new()
        {
            return SpawnInternal(new TActor(), alias, mailboxCapacity).Self;
        }
        public IActorRef Spawn<TActor>(Func<string, TActor> factory,
            string alias = null,
            int mailboxCapacity = DefaultMailboxCapacity
            ) where TActor : ActorBase
        {
            return SpawnInternal(factory(alias), alias, mailboxCapacity).Self;
        }
        public IActorRef Spawn<TActor>(Func<TActor> factory, 
            int mailboxCapacity = DefaultMailboxCapacity
            ) where TActor : ActorBase
        {
            return SpawnInternal(factory(), null, mailboxCapacity).Self;
        }

        internal TActor SpawnInternal<TActor>(TActor actor, string alias, int mailboxCapacity) where TActor : ActorBase
        {
            int id = Interlocked.Increment(ref _nextActorId);
            int dispatcherIndex = id % _dispatchers.Length;
            var dispatcher = _dispatchers[dispatcherIndex];

            var runner = new ActorRunner(actor, dispatcher, mailboxCapacity, FinalizeKill);

            ActorRef actorRef = new(this, id, alias);
            actor.Initialize(dispatcher, actorRef);

            try
            {
                if (_actorRunners.TryAdd(id, runner) == false)
                {
                    throw new InvalidOperationException($"Duplicate actor id.{id}");
                }

                if (string.IsNullOrEmpty(alias) == false)
                {
                    if (_aliasToId.TryAdd(alias, id) == false)
                    {
                        _actorRunners.TryRemove(id, out _);
                        throw new InvalidOperationException($"Duplicate actor alias.{alias}");
                    }
                }
            }
            catch(Exception)
            {
                runner.Kill();
                throw;
            }

            return actor;
        }
        internal void Post(int actorId, in ActorMail actorMail)
        {
            if (!_actorRunners.TryGetValue(actorId, out var actorRunner))
            {
                PublishDeadLetter(actorMail.Message,
                    actorMail.Sender,
                    actorId,
                    DeadLetterReason.RecipientInvalidated);
                return;
            }

            var result = actorRunner.Enqueue(actorMail);

            if (result == Internals.EnqueueResult.Success)
            {
                return;
            }

            switch (result)
            {
                case Internals.EnqueueResult.MailboxFull:
                    PublishDeadLetter(actorMail.Message,
                        actorMail.Sender,
                        actorId,
                        DeadLetterReason.MailboxFull);
                    break;
                case Internals.EnqueueResult.ActorStopped:
                    PublishDeadLetter(actorMail.Message,
                        actorMail.Sender,
                        actorId,
                        DeadLetterReason.ActorStopped);
                    break;
            }
        }
        internal void Post(int actorId, IActorMessage message, IActorRef sender)
        {
            Post(actorId, new ActorMail(message, sender));
        }
        internal void Kill(int actorId)
        {
            if (_actorRunners.TryGetValue(actorId, out var actorRunner) == true)
            {
                actorRunner.Kill();
            }
        }
        internal void FinalizeKill(int actorId)
        {
            if(_actorRunners.TryRemove(actorId, out var actorRunner))
            {
                var actor = actorRunner.GetActor();

                if(!string.IsNullOrEmpty(actor.SelfActorRef.Alias))
                {
                    _aliasToId.TryRemove(actor.SelfActorRef.Alias, out _);
                }
            }
        }

        bool IActorRefProvider.TryGetActorRef(int id, out IActorRef actorRef)
        {
            return TryGetActorRef(id, out actorRef);
        }
        internal bool TryGetActorRef(int id, out IActorRef actorRef)
        {
            actorRef = null;
            if (_actorRunners.TryGetValue(id, out var actorRunner))
            {
                actorRef = actorRunner.GetActor().Self;
                return true;
            }
            return false;
        }

        public bool TryGetActorRef(string alias, out IActorRef actorRef)
        {
            actorRef = null;
            if (_aliasToId.TryGetValue(alias, out int id))
            {
                return TryGetActorRef(id, out actorRef);
            }
            return false;
        }
        internal void PublishDeadLetter(IActorMessage message, IActorRef sender, int recipientActorId, DeadLetterReason reason)
        {
            PublishDeadLetter(new DeadLetterMessage(message, sender, recipientActorId, reason));
        }
        internal void PublishDeadLetter(DeadLetterMessage message)
        {
            OnDeadLetterDetected?.Invoke(message);
        }
    }
}
