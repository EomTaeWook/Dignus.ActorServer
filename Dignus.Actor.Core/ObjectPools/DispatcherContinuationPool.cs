// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Core.Internals;
using Dignus.Framework;

namespace Dignus.Actor.Core.ObjectPools
{
    internal class DispatcherContinuationPool
    {
        private class InnerPool : ObjectPoolBase<DispatcherContinuation>
        {
            private readonly DispatcherContinuationPool _parentPool;
            public InnerPool(DispatcherContinuationPool parentPool)
            {
                _parentPool = parentPool;
            }
            public override DispatcherContinuation CreateItem()
            {
                var item = new DispatcherContinuation(_parentPool);
                return item;
            }
            public override void Remove(DispatcherContinuation item)
            {
            }
        }

        private readonly InnerPool _innerPool;

        public DispatcherContinuationPool()
        {
            _innerPool = new InnerPool(this);
        }

        public DispatcherContinuation Pop()
        {
            lock (_innerPool)
            {
                return _innerPool.Pop();
            }
        }

        public void Push(DispatcherContinuation item)
        {
            lock (_innerPool)
            {
                _innerPool.Push(item);
            }
        }
    }
}
