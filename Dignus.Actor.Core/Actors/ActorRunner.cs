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

    internal class ActorRunner(ActorBase actor, ActorDispatcher dispatcher) : IActorSchedulable
    {
        private readonly SynchronizedArrayQueue<ActorMail> _mailbox = [];
        private int _scheduled;
        public void Enqueue(IActorMessage msg, IActorRef sender)
        {
            var mail = dispatcher.MailPool.Pop();
            mail.Message = msg;
            mail.Sender = sender;
            _mailbox.Add(mail);

            if (Interlocked.CompareExchange(ref _scheduled, 1, 0) == 0)
            {
                dispatcher.Schedule(this);
            }
        }
        internal void VerifyContext()
        {
            var actorDispatcher = ActorDispatcher.CurrentActorDispatcher ?? throw new InvalidOperationException($"Actor P-{dispatcher.Id} is running on ThreadPool.");

            if (actorDispatcher.Id != dispatcher.Id)
            {
                throw new InvalidOperationException($"Actor P-{dispatcher.Id} vs Current P-{actorDispatcher.Id}");
            }
        }
        public async Task ExecuteAsync()
        {
            VerifyContext();

            while (_mailbox.TryRead(out var mail))
            {
                Task task;
                try
                {
                    task = actor.OnReceiveInternal(mail.Message, mail.Sender);
                }
                catch
                {
                    mail.Recycle();
                    throw;
                }

                if (task.IsCompleted)
                {
                    if (task.IsFaulted)
                    {
                        throw task.Exception;
                    }

                    mail.Recycle();
                    continue;
                }

                try
                {
                    await task.ConfigureAwait(true);
                }
                finally
                {
                    mail.Recycle();
                }
                break;
            }

            Interlocked.Exchange(ref _scheduled, 0);
            if (_mailbox.Count > 0 && Interlocked.CompareExchange(ref _scheduled, 1, 0) == 0)
            {
                dispatcher.Schedule(this);
            }
        }
    }
}
