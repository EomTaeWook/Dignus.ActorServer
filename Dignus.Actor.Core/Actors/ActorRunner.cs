using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Messages;
using Dignus.Collections;
using System;
using System.Threading;

namespace Dignus.Actor.Core.Actors
{
    internal sealed class ActorRunner : IActorSchedulable
    {
        private readonly ActorBase _actor;
        private readonly ActorDispatcher _dispatcher;
        private readonly Action<int> _onFinalize;
        private readonly SynchronizedArrayQueue<ActorMail> _mailbox = [];

        private int _actorState;

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
            if (GetState() >= ActorState.Stopping)
            {
                return;
            }

            ActorMail actorMail = new()
            {
                Message = message,
                Sender = sender
            };

            _mailbox.Add(actorMail);
            if(_actorState == 1)
            {
                return;
            }
            TrySchedule();
        }

        public void Kill()
        {
            if (GetState() >= ActorState.Stopping)
            {
                return;
            }
            if (TryTransition(ActorState.Idle, ActorState.Stopping) 
                || TryTransition(ActorState.Idle, ActorState.Stopping))
            {
                TrySchedule();
            }
        }

        public void Execute()
        {
            while (_mailbox.TryRead(out ActorMail actorMail))
            {
                if (GetState() == ActorState.Stopping)
                {
                    break;
                }

                var receiveTask = _actor.OnReceiveInternal(actorMail.Message, actorMail.Sender);

                if (receiveTask.IsCompleted)
                {
                    if (receiveTask.IsFaulted)
                    {
                        throw receiveTask.AsTask().Exception;
                    }

                    continue;
                }

                receiveTask.ConfigureAwait(false);

                return;
            }

            if (GetState() == ActorState.Stopping)
            {
                FinalizeKill();
                return;
            }

            if (_mailbox.Count > 0)
            {
                _dispatcher.Schedule(this);
                return;
            }

            TryTransition(ActorState.Pending, ActorState.Idle);
        }
        private void TrySchedule()
        {
            if(TryTransition(ActorState.Idle, ActorState.Pending))
            {
                _dispatcher.Schedule(this);
            }
            else
            {
                Console.WriteLine("faield to Schedule");
            }
        }

        private ActorState GetState()
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

            while (_mailbox.TryRead(out ActorMail _))
            {
                //actorMail.Recycle();
            }
            _actor.Cleanup();
            _onFinalize(_actor.SelfRef.Id);
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
