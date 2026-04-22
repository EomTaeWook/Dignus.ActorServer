// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Internals;
using Dignus.Framework;

namespace Dignus.Actor.Core.ObjectPools
{
    internal class ActorYieldTaskPool
    {
        private class InnerPool : ObjectPoolBase<ActorYieldTask>
        {
            private readonly ActorYieldTaskPool _parentPool;
            public InnerPool(ActorYieldTaskPool parentPool)
            {
                _parentPool = parentPool;
            }
            public override ActorYieldTask CreateItem()
            {
                var item = new ActorYieldTask(_parentPool);
                return item;
            }
            public override void Remove(ActorYieldTask item)
            {
            }
        }

        private readonly InnerPool _innerPool;

        public ActorYieldTaskPool()
        {
            _innerPool = new InnerPool(this);
        }

        public ActorYieldTask Pop()
        {
            return _innerPool.Pop();
        }

        public void Push(ActorYieldTask item)
        {
            _innerPool.Push(item);
        }
    }
}
