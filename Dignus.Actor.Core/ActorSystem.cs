// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Abstractions;
using Dignus.Actor.Core.DeadLetter;
using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Internals;
using Dignus.Actor.Core.Messages;
using Dignus.Collections;
using Dignus.DependencyInjection.Attributes;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Dignus.Actor.Core
{
    public class ActorSystem : IActorRefProvider, IDeadLetterPublisher
    {
        public event Action<DeadLetterMessage> OnDeadLetterDetected;

        const int DefaultMailboxCapacity = 1024;
        public int DispatcherCount => _dispatchers.Length;

        private readonly ConcurrentDictionary<long, ActorRunner> _actorRunners = new();
        private readonly ConcurrentDictionary<string, long> _aliasToId = new();
        private readonly ActorDispatcher[] _dispatchers;
        private int _nextActorId;
        private int _isDisposed;

        [InjectConstructor]
        public ActorSystem() :this(Environment.ProcessorCount)
        {
        }
        public ActorSystem(int dispatcherThreadCount)
        {
            _dispatchers = new ActorDispatcher[dispatcherThreadCount];
            for (int i = 0; i < dispatcherThreadCount; i++)
            {
                _dispatchers[i] = new ActorDispatcher(i);
                _dispatchers[i].Start();
            }
        }
        public IActorRef SpawnOnDispatcher<TActor>(int dispatcherIndex, string alias = null, int mailboxCapacity = DefaultMailboxCapacity) 
            where TActor : ActorBase, new()
        {
            return SpawnWithDispatcher(new TActor(), dispatcherIndex, alias, mailboxCapacity).Self;
        }

        public IActorRef SpawnOnDispatcher<TActor>(Func<TActor> factory, int dispatcherIndex, string alias = null, int mailboxCapacity = DefaultMailboxCapacity) 
            where TActor : ActorBase
        {
            return SpawnWithDispatcher(factory(), dispatcherIndex, alias, mailboxCapacity).Self;
        }
        internal TActor SpawnWithDispatcher<TActor>(TActor actor, int dispatcherIndex, string alias, int mailboxCapacity) 
            where TActor : ActorBase
        {
            ThrowIfDisposed();

            if ((uint)dispatcherIndex >= (uint)_dispatchers.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(dispatcherIndex));
            }

            int actorId = Interlocked.Increment(ref _nextActorId);
            ActorDispatcher actorDispatcher = _dispatchers[dispatcherIndex];
            return RegisterActor(actor, actorId, actorDispatcher, alias, mailboxCapacity);
        }

        public IActorRef Spawn<TActor>(string alias = null, int mailboxCapacity = DefaultMailboxCapacity) 
            where TActor : ActorBase, new()
        {
            return SpawnWithAutoDispatcher(new TActor(), alias, mailboxCapacity).Self;
        }

        public IActorRef Spawn<TActor>(Func<TActor> factory, string alias = null, int mailboxCapacity = DefaultMailboxCapacity)
            where TActor : ActorBase
        {
            return SpawnWithAutoDispatcher(factory(), alias, mailboxCapacity).Self;
        }

        internal TActor SpawnWithAutoDispatcher<TActor>(TActor actor, string alias, int mailboxCapacity) where TActor : ActorBase
        {
            ThrowIfDisposed();

            int actorId = Interlocked.Increment(ref _nextActorId);
            int dispatcherIndex = actorId % _dispatchers.Length;
            var dispatcher = _dispatchers[dispatcherIndex];
            return RegisterActor(actor, actorId, dispatcher, alias, mailboxCapacity);
        }
        private void ThrowIfDisposed()
        {
            if (_isDisposed == 1)
            {
                throw new ObjectDisposedException(nameof(ActorSystem));
            }
        }
        private TActor RegisterActor<TActor>(TActor actor, int actorId, ActorDispatcher actorDispatcher, string alias, int mailboxCapacity)
            where TActor : ActorBase
        {
            var actorRunner = new ActorRunner(actor, actorDispatcher, mailboxCapacity, this, FinalizeKill);
            var actorRef = new ActorRef(this, actorId, alias);
            actor.Initialize(actorDispatcher, actorRef);

            try
            {
                if (_actorRunners.TryAdd(actorId, actorRunner) == false)
                {
                    throw new InvalidOperationException($"Duplicate actor id.{actorId}");
                }

                if (string.IsNullOrEmpty(alias) == false)
                {
                    if (_aliasToId.TryAdd(alias, actorId) == false)
                    {
                        _actorRunners.TryRemove(actorId, out _);
                        throw new InvalidOperationException($"Duplicate actor alias.{alias}");
                    }
                }
            }
            catch (Exception)
            {
                actorRunner.Kill();
                throw;
            }

            return actor;
        }
        internal void Post(int actorId, in ActorMail actorMail)
        {
            if (_isDisposed == 1)
            {
                PublishDeadLetter(actorMail.Message, actorMail.Sender, actorId, DeadLetterReason.ActorSystemDisposed);
                return;
            }

            if (_actorRunners.TryGetValue(actorId, out var actorRunner) == false)
            {
                PublishDeadLetter(actorMail.Message, actorMail.Sender, actorId, DeadLetterReason.RecipientInvalidated);
                return;
            }

            var result = actorRunner.Enqueue(actorMail);
            if (result == EnqueueResult.Success)
            {
                return;
            }
            switch (result)
            {
                case EnqueueResult.MailboxFull:
                    PublishDeadLetter(actorMail.Message, actorMail.Sender, actorId, DeadLetterReason.MailboxFull);
                    break;
                case EnqueueResult.ActorStopped:
                    PublishDeadLetter(actorMail.Message, actorMail.Sender,actorId,DeadLetterReason.ActorStopped);
                    break;
            }
        }
        internal void Post(int actorId, IActorMessage message, IActorRef sender)
        {
            Post(actorId, new ActorMail(message, sender));
        }
        internal void Kill(int actorId)
        {
            if (_actorRunners.TryGetValue(actorId, out var actorRunner))
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

        bool IActorRefProvider.TryGetActorRef(long id, out IActorRef actorRef)
        {
            return TryGetActorRef(id, out actorRef);
        }
        internal bool TryGetActorRef(long id, out IActorRef actorRef)
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
            if (_aliasToId.TryGetValue(alias, out long id))
            {
                return TryGetActorRef(id, out actorRef);
            }
            return false;
        }
        void IDeadLetterPublisher.Publish(DeadLetterMessage deadLetterMessage)
        {
            PublishDeadLetter(deadLetterMessage);
        }
        internal void PublishDeadLetter(IActorMessage message, IActorRef sender, int recipientActorId, DeadLetterReason reason)
        {
            PublishDeadLetter(new DeadLetterMessage(message, sender, recipientActorId, reason));
        }
        private void PublishDeadLetter(DeadLetterMessage deadLetterMessage)
        {
            OnDeadLetterDetected?.Invoke(deadLetterMessage);
        }
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 1)
            {
                return;
            }

            var copy = new ArrayQueue<ActorRunner>(_actorRunners.Values.Count);
            copy.AddRange(_actorRunners.Values);
            foreach (var actor in copy)
            {
                actor.Kill();
            }

            while (!_actorRunners.IsEmpty)
            {
                Thread.Yield();
            }

            foreach (var dispatcher in _dispatchers)
            {
                dispatcher.Dispose();
            }
        }
    }
}
