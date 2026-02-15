// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Internals;
using Dignus.Actor.Core.Messages;
using Dignus.Collections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Actors
{
    internal sealed class ActorRunner(ActorBase actor,
        ActorDispatcher dispatcher,
        int mailboxCapacity,
        Action<int> onFinalize
        ) : IActorSchedulable
    {
        internal static void EnqueueContinuation(object state)
        {
            var runner = (ActorRunner)state;
            try
            {
                var task = Volatile.Read(ref runner._pendingReceiveTask);
                task.GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                runner.Kill();
            }
            finally
            {
                Volatile.Write(ref runner._pendingReceiveTask, null);
                runner._dispatcher.Schedule(runner);
            }
        }
        private readonly ActorBase _actor = actor;
        private readonly ActorDispatcher _dispatcher = dispatcher;
        private readonly Action<int> _onFinalize = onFinalize;
        private readonly MpscBoundedQueue<ActorMail> _mailbox = new(mailboxCapacity);

        private int _isScheduled;
        private int _lifecycleState = 0;
        private Task _pendingReceiveTask;

        public ActorBase GetActor()
        {
            return _actor;
        }
        public EnqueueResult Enqueue(IActorMessage message, IActorRef sender)
        {
            ActorMail actorMail = new(message, sender);
            return Enqueue(actorMail);
        }
        public EnqueueResult Enqueue(in ActorMail actorMail)
        {
            if (Volatile.Read(ref _lifecycleState) != 0)
            {
                return EnqueueResult.ActorStopped;
            }

            if(_mailbox.TryEnqueue(actorMail) == false)
            {
                return EnqueueResult.MailboxFull;
            }

            if (Interlocked.Exchange(ref _isScheduled, 1) == 0)
            {
                _dispatcher.Schedule(this);
            }

            return EnqueueResult.Success;
        }

        public void Kill()
        {
            if(Interlocked.CompareExchange(ref _lifecycleState, 1, 0) == 0)
            {
                if (Interlocked.Exchange(ref _isScheduled, 1) == 0)
                {
                    _dispatcher.Schedule(this);
                }
            }
        }

        public void Execute()
        {
            if (Volatile.Read(ref _pendingReceiveTask) != null)
            {
                return;
            }

            while (_mailbox.TryDequeue(out ActorMail actorMail))
            {
                if (_lifecycleState > 0)
                {
                    break;
                }

                var valueTask = _actor.OnReceiveInternal(actorMail.Message,
                    actorMail.Sender);

                if (valueTask.IsCompleted)
                {
                    continue;
                }

                _pendingReceiveTask = valueTask.AsTask();

                _pendingReceiveTask.ContinueWith((task, state) => 
                {
                    var runner = (ActorRunner)state;
                    runner._dispatcher.EnqueueContinuation(EnqueueContinuation, state);
                },
                this);
                return;
            }

            if (Volatile.Read(ref _lifecycleState) == 1)
            {
                FinalizeKill();
                return;
            }

            Volatile.Write(ref _isScheduled, 0);

            if (_mailbox.TryPeek(out _))
            {
                if (Interlocked.Exchange(ref _isScheduled, 1) == 0)
                {
                    _dispatcher.Schedule(this);
                }
            }
        }

        private void FinalizeKill()
        {
            VerifyContext();

            Interlocked.Exchange(ref _lifecycleState, 2);

            while(_mailbox.TryDequeue(out _))
            {
            }

            _actor.Cleanup();
            _actor.OnKill();
            _onFinalize(_actor.SelfActorRef.Id);
        }

        internal void VerifyContext()
        {
            ActorDispatcher actorDispatcher =
                ActorDispatcher.CurrentActorDispatcher
                ?? throw new InvalidOperationException($"Actor P-{_dispatcher.Id} is running on ThreadPool.");

            if (actorDispatcher.Id != _dispatcher.Id)
            {
                throw new InvalidOperationException($"Actor P-{_dispatcher.Id} vs Current P-{actorDispatcher.Id}");
            }
        }
    }
}
