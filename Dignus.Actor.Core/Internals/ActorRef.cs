// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Abstractions;
using Dignus.Actor.Core.Messages;
using System;
using System.Threading.Tasks;

namespace Dignus.Actor.Core.Internals
{
    internal class ActorRef : IAskActorRef
    {
        public long ActorId { get => _actorId; }
        public string Alias { get => _alias; }

        private readonly ActorSystem _actorSystem;
        private readonly long _actorId;
        private readonly string _alias;

        public ActorRef(ActorSystem actorSystem, long actorId, string alias)
        {
            _actorSystem = actorSystem;
            _actorId = actorId;
            _alias = alias;
        }

        public void Post(IActorMessage message, IActorRef sender)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            _actorSystem.Post(_actorId, message, sender);
        }
        public void Post(in ActorMail actorMail)
        {
            _actorSystem.Post(_actorId, in actorMail);
        }

        public void Kill()
        {
            _actorSystem.Kill(_actorId);
        }
        public ValueTask<TResponse> AskAsync<TResponse>(IActorMessage message, int timeoutMilliseconds) where TResponse : IActorMessage
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var askReplyActorRef = _actorSystem.AskSystem.Register<TResponse>(TimeSpan.FromMilliseconds(timeoutMilliseconds));

            Post(message, askReplyActorRef);

            return askReplyActorRef.ValueTask;
        }
    }
}
