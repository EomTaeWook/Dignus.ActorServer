// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Messages;
using Dignus.Framework;

namespace Dignus.Actor.Core.ObjectPools
{
    internal class ActorMailPool
    {
        private class InnerPool(ActorMailPool parent) : ObjectPoolBase<ActorMail>
        {
            public override ActorMail CreateItem()
            {
                var item = new ActorMail(parent);
                return item;
            }
            public override void Remove(ActorMail item)
            {
            }
        }
        private readonly InnerPool _pool;

        public ActorMailPool()
        {
            _pool = new InnerPool(this);
        }
        
        public ActorMail Pop()
        {
            var item = _pool.Pop();
            return item;
        }

        public void Push(ActorMail item)
        {
            if(item == null)
            {

            }
            _pool.Push(item);
        }
    }
}
