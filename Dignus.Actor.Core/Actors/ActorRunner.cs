// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Dispatcher;
using Dignus.Actor.Core.Messages;
using Dignus.Collections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Actors
{
    internal interface IActorSchedulable
    {
        Task ExecuteAsync();
    }

    internal class ActorRunner : IActorSchedulable
    {
        private readonly ActorDispatcher _dispatcher;
        private readonly ActorBase _actor;

        private readonly SynchronizedArrayQueue<ActorMail> _mailbox = new SynchronizedArrayQueue<ActorMail>();
        private int _scheduled;

        public ActorRunner(ActorBase actor, ActorDispatcher dispatcher)
        {
            _actor = actor;
            _dispatcher = dispatcher;
        }
        public void Enqueue(IActorMessage msg, IActorRef sender)
        {
            var mail = _dispatcher.MailPool.Pop();
            mail.Message = msg;
            mail.Sender = sender;
            _mailbox.Add(mail);

            if (Interlocked.CompareExchange(ref _scheduled, 1, 0) == 0)
            {
                _dispatcher.Schedule(this);
            }
        }
        internal void VerifyContext()
        {
            var actorDispatcher = ActorDispatcher.CurrentActorDispatcher;
            if (actorDispatcher == null)
            {
                throw new InvalidOperationException($"Actor P-{_dispatcher.Id} is running on ThreadPool!");
            }

            if (actorDispatcher.Id != _dispatcher.Id)
            {
                throw new InvalidOperationException($"Actor P-{_dispatcher.Id} vs Current P-{actorDispatcher.Id}");
            }
        }
        public async Task ExecuteAsync()
        {
            VerifyContext();
            try
            {
                while(_mailbox.TryRead(out var mail))
                {
                    try
                    {
                        await _actor.OnReceiveInternal(mail.Message, mail.Sender).ConfigureAwait(true);
                    }
                    finally
                    {
                        mail.Recycle();
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _scheduled, 0);
                if (_mailbox.Count > 0 && Interlocked.CompareExchange(ref _scheduled, 1, 0) == 0)
                {
                    _dispatcher.Schedule(this);
                }
            }
        }
    }
}
