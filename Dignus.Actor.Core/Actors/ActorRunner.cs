using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Messages;
using Dignus.Collections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Actors
{
    internal sealed class ActorRunner(ActorBase actor, ActorDispatcher dispatcher, Action<int> onFinalize) : IActorSchedulable
    {
        private readonly ActorBase _actor = actor;
        private readonly ActorDispatcher _dispatcher = dispatcher;
        private readonly Action<int> _onFinalize = onFinalize;
        private readonly SynchronizedArrayQueue<ActorMail> _mailbox = [];

        private int _hasMail;
        private int _lifecycleState = 0;

        public IActorRef GetActorRef()
        {
            return _actor.Self;
        }
        public void Enqueue(IActorMessage message, IActorRef sender)
        {
            if (Volatile.Read(ref _lifecycleState) != 0)
            {
                return;
            }

            ActorMail actorMail = new(message, sender);

            _mailbox.Add(actorMail);

            if (Interlocked.Exchange(ref _hasMail, 1) == 0)
            {
                _dispatcher.Schedule(this);
            }
        }

        public void Kill()
        {
            if(Interlocked.CompareExchange(ref _lifecycleState, 1, 0) == 0)
            {
                if (Interlocked.Exchange(ref _hasMail, 1) == 0)
                {
                    _dispatcher.Schedule(this);
                }
            }
        }

        public void Execute()
        {
            while (_mailbox.TryRead(out ActorMail actorMail))
            {
                if (_lifecycleState > 0)
                {
                    break;
                }

                var valueTask = _actor.OnReceiveInternal(actorMail.Message,
                    actorMail.Sender);

                if (!valueTask.IsCompleted)
                {
                    break;
                }
            }

            if (Volatile.Read(ref _lifecycleState) == 1)
            {
                FinalizeKill();
                return;
            }

            Volatile.Write(ref _hasMail, 0);

            var mail = _mailbox.Peek();
            if (mail.Message != null)
            {
                if (Interlocked.Exchange(ref _hasMail, 1) == 0)
                {
                    _dispatcher.Schedule(this);
                }
            }
        }

        private void FinalizeKill()
        {
            VerifyContext();

            Interlocked.Exchange(ref _lifecycleState, 2);

            _mailbox.Clear();
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
