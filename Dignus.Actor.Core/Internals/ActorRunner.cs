// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Messages;
using Dignus.Collections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Internals
{
    internal class ActorRunner(ActorBase actor,
        ActorDispatcher dispatcher,
        int mailboxCapacity,
        Action<int> onFinalize
        ) : IActorSchedulable
    {
        internal static void ContinuationAction(Task completedTask, object state)
        {
            var runner = (ActorRunner)state;

            if (completedTask.IsFaulted)
            {
                runner.Kill();
            }
            Volatile.Write(ref runner._pendingReceiveTask, null);
            runner._dispatcher.Schedule(runner);
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

                ValueTask valueTask;
                try
                {
                    valueTask = _actor.OnReceiveInternal(actorMail.Message,
                    actorMail.Sender);
                }
                catch(OperationCanceledException)
                {
                    continue;
                }
                catch(Exception)
                {
                    Kill();
                    break;
                }

                if (valueTask.IsCompleted)
                {
                    if(valueTask.IsFaulted)
                    {
                        Kill();
                        break;
                    }

                    continue;
                }

                Task receiveTask = valueTask.AsTask();
                Volatile.Write(ref _pendingReceiveTask, receiveTask);
                receiveTask.ContinueWith(ContinuationAction,
                    this,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
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
            if (Interlocked.Exchange(ref _lifecycleState, 2) == 2)
            {
                return;
            }
            while (_mailbox.TryDequeue(out _))
            {
            }
            _actor.KillInternal();
            _onFinalize(_actor.SelfActorRef.Id);
        }
    }
}
