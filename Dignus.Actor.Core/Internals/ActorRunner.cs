// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Abstractions;
using Dignus.Actor.Core.DeadLetter;
using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Messages;
using Dignus.Collections;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Internals
{
    internal class ActorRunner : IActorSchedulable
    {
        private readonly ActorBase _actor;
        private readonly ActorDispatcher _dispatcher;
        private readonly IDeadLetterPublisher _deadLetterPublisher;
        private readonly Action<long> _onFinalize;
        private readonly MpscBoundedQueue<ActorMail> _mailbox;
        private readonly Action _receiveCompletedDelegate;

        private int _isScheduled;
        private int _lifecycleState = 0;

        private int _hasPendingReceive;
        private TaskAwaiter _pendingAwaiter;

        public ActorRunner(ActorBase actor,
            ActorDispatcher dispatcher,
            int mailboxCapacity,
            IDeadLetterPublisher deadLetterPublisher,
            Action<long> onFinalize)
        {
            _actor = actor;
            _dispatcher = dispatcher;
            _mailbox = new MpscBoundedQueue<ActorMail>(mailboxCapacity);
            _deadLetterPublisher = deadLetterPublisher;
            _onFinalize = onFinalize;
            _receiveCompletedDelegate = OnReceiveCompleted;

        }
        public ActorBase GetActor()
        {
            return _actor;
        }
        public EnqueueResult Enqueue(IActorMessage message, IActorRef sender)
        {
            ActorMail actorMail = new ActorMail(message, sender);
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
            if (Volatile.Read(ref _hasPendingReceive) != 0)
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
                    valueTask = _actor.OnReceiveInternal(actorMail.Message, actorMail.Sender);
                }
                catch(OperationCanceledException)
                {
                    continue;
                }
                catch(Exception ex)
                {
                    Kill();
                    PublishExecutionException(ex);
                    break;
                }

                if (valueTask.IsCompleted)
                {
                    if (valueTask.IsFaulted)
                    {
                        var ex = valueTask.AsTask().Exception;
                        if (ex != null)
                        {
                            PublishExecutionException(ex);
                        }

                        Kill();
                        break;
                    }

                    continue;
                }

                _pendingAwaiter = valueTask.AsTask().GetAwaiter();
                Volatile.Write(ref _hasPendingReceive, 1);

                try
                {
                    _pendingAwaiter.UnsafeOnCompleted(_receiveCompletedDelegate);
                }
                catch (Exception ex)
                {
                    _pendingAwaiter = default;
                    Volatile.Write(ref _hasPendingReceive, 0);

                    Kill();
                    PublishExecutionException(ex);
                    break;
                }
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
        private void OnReceiveCompleted()
        {
            try
            {
                _pendingAwaiter.GetResult();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Kill();
                PublishExecutionException(ex);
            }
            finally
            {
                _pendingAwaiter = default;
                Volatile.Write(ref _hasPendingReceive, 0);
                _dispatcher.Schedule(this);
            }
        }
        private void PublishExecutionException(Exception ex)
        {
            _deadLetterPublisher.Publish(new DeadLetterMessage(
                new ActorExceptionMessage(ex),
                null,
                _actor.SelfActorRef.Id,
                DeadLetterReason.ExecutionException));
        }
    }
}
