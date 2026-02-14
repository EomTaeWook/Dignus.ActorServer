// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Internals;
using Dignus.Framework;

namespace Dignus.Actor.Core.ObjectPools
{
    internal class ActorYieldTaskPool
    {
        private class InnerPool(ActorYieldTaskPool parent) : ObjectPoolBase<ActorYieldTask>
        {
            public override ActorYieldTask CreateItem()
            {
                var item = new ActorYieldTask(parent);
                return item;
            }
            public override void Remove(ActorYieldTask item)
            {
            }
        }

        private readonly InnerPool _pool;

        public ActorYieldTaskPool()
        {
            _pool = new InnerPool(this);
        }

        public ActorYieldTask Pop()
        {
            return _pool.Pop();
        }

        public void Push(ActorYieldTask item)
        {
            _pool.Push(item);
        }
    }
}
