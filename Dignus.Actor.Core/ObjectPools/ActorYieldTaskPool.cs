// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Actor.Core.Internals;
using Dignus.Framework;

namespace Dignus.Actor.Core.ObjectPools
{
    internal class ActorYieldTaskPool
    {
        private class InnerYieldTaskPool(ActorYieldTaskPool parent) : ObjectPoolBase<ActorYieldTask>
        {
            public override ActorYieldTask CreateItem()
            {
                var item = new ActorYieldTask();
                item.SetPool(parent);
                return item;
            }
            public override void Remove(ActorYieldTask item)
            {
            }
        }

        private readonly InnerYieldTaskPool _pool;

        public ActorYieldTaskPool()
        {
            _pool = new InnerYieldTaskPool(this);
        }

        public ActorYieldTask Pop() => _pool.Pop();

        public void Push(ActorYieldTask item)
        {
            item.Clear();
            _pool.Push(item);
        }
    }
}
