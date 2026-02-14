using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Messages;
using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Actors
{
    internal sealed class ActorRunner : IActorSchedulable
    {
        private readonly ActorBase _actor;
        private readonly ActorDispatcher _dispatcher;
        private readonly Action<int> _onFinalize;

        private readonly ConcurrentQueue<ActorMail> _mailbox = new();

        private int _isScheduled;
        private int _actorState;

        // async 대기 중인 mail/task 관리
        private int _isAwaiting;
        private ActorMail _pendingMail;
        private Task _pendingTask;
        private ExceptionDispatchInfo _pendingException;

        public ActorRunner(ActorBase actor, ActorDispatcher dispatcher, Action<int> onFinalize)
        {
            _actor = actor;
            _dispatcher = dispatcher;
            _onFinalize = onFinalize;
        }

        public IActorRef GetActorRef()
        {
            return _actor.Self;
        }

        public void Enqueue(IActorMessage message, IActorRef sender)
        {
            if (ReadState() != ActorState.Active)
            {
                return;
            }

            ActorMail actorMail = _dispatcher.MailPool.Pop();
            actorMail.Message = message;
            actorMail.Sender = sender;

            _mailbox.Enqueue(actorMail);
            TrySchedule();
        }

        public void Kill()
        {
            if (!TryTransition(ActorState.Active, ActorState.Stopping))
            {
                return;
            }

            TrySchedule();
        }

        public void Execute()
        {
            VerifyContext();

            // 이전 async 작업 예외를 dispatcher 스레드에서 터뜨려 기존 동작 유지
            ExceptionDispatchInfo pendingException = _pendingException;
            if (pendingException != null)
            {
                _pendingException = null;
                pendingException.Throw();
            }

            // async 대기 중이면, 완료 콜백이 스케줄할 때까지 실행하지 않음
            if (Volatile.Read(ref _isAwaiting) != 0)
            {
                Interlocked.Exchange(ref _isScheduled, 0);
                return;
            }

            while (_mailbox.TryDequeue(out ActorMail actorMail))
            {
                if (ReadState() == ActorState.Stopping)
                {
                    actorMail.Recycle();
                    break;
                }

                Task receiveTask;
                try
                {
                    receiveTask = _actor.OnReceiveInternal(actorMail.Message, actorMail.Sender);
                }
                catch
                {
                    actorMail.Recycle();
                    throw;
                }

                if (receiveTask.IsCompleted)
                {
                    // 동기 완료 fast-path
                    if (receiveTask.IsFaulted)
                    {
                        actorMail.Recycle();
                        // AggregateException 그대로 던지는 기존 스타일 유지(원하면 여기서 정리 가능)
                        throw receiveTask.Exception;
                    }

                    actorMail.Recycle();
                    continue;
                }

                // 비동기 경로: mail을 잡아두고 완료 시점에 다시 스케줄
                _pendingMail = actorMail;
                _pendingTask = receiveTask;
                Volatile.Write(ref _isAwaiting, 1);

                receiveTask.ContinueWith(
                    static (Task completedTask, object state) =>
                    {
                        ActorRunner actorRunner = (ActorRunner)state;
                        actorRunner.OnReceiveTaskCompleted(completedTask);
                    },
                    this,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);

                break;
            }

            Interlocked.Exchange(ref _isScheduled, 0);

            if (ReadState() == ActorState.Stopping)
            {
                FinalizeKill();
                return;
            }

            if (!_mailbox.IsEmpty)
            {
                TrySchedule();
            }
        }

        private void OnReceiveTaskCompleted(Task completedTask)
        {
            _dispatcher.EnqueueContinuation(
                static state =>
                {
                    ActorRunner actorRunner = (ActorRunner)state;
                    actorRunner.CompletePendingOnDispatcher();
                },
                this);
        }

        private void CompletePendingOnDispatcher()
        {
            VerifyContext();

            ActorMail pendingMail = _pendingMail;
            Task pendingTask = _pendingTask;

            _pendingMail = null;
            _pendingTask = null;

            try
            {
                if (pendingTask.IsFaulted)
                {
                    _pendingException = ExceptionDispatchInfo.Capture(pendingTask.Exception);
                }
            }
            finally
            {
                pendingMail?.Recycle();
                Volatile.Write(ref _isAwaiting, 0);
            }

            TrySchedule();
        }

        private void TrySchedule()
        {
            if (Interlocked.CompareExchange(ref _isScheduled, 1, 0) == 0)
            {
                _dispatcher.Schedule(this);
            }
        }

        private ActorState ReadState()
        {
            return (ActorState)Volatile.Read(ref _actorState);
        }

        private bool TryTransition(ActorState expected, ActorState desired)
        {
            return Interlocked.CompareExchange(
                       ref _actorState,
                       (int)desired,
                       (int)expected) == (int)expected;
        }

        private void FinalizeKill()
        {
            VerifyContext();

            if (!TryTransition(ActorState.Stopping, ActorState.Stopped))
            {
                return;
            }

            while (_mailbox.TryDequeue(out ActorMail actorMail))
            {
                actorMail.Recycle();
            }

            // pending 정리
            ActorMail pendingMail = _pendingMail;
            _pendingMail = null;
            _pendingTask = null;
            _pendingException = null;
            Volatile.Write(ref _isAwaiting, 0);

            pendingMail?.Recycle();

            _actor.Cleanup();
            _onFinalize(_actor.SelfRef.Id);

            Interlocked.Exchange(ref _isScheduled, 0);
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
