// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Actor.Core.Internals;
using Dignus.Collections;

namespace Dignus.Actor.Core.ObjectPools
{
    internal class DispatcherContinuationPool
    {
        private class InnerPool
        {
            private readonly DispatcherContinuationPool _parentPool;
            private readonly ArrayQueue<DispatcherContinuation> _itemContainer = new ArrayQueue<DispatcherContinuation>();
            public InnerPool(DispatcherContinuationPool parentPool)
            {
                _parentPool = parentPool;
            }
            public DispatcherContinuation Pop()
            {
                if (_itemContainer.TryRead(out DispatcherContinuation item))
                {
                    return item;
                }

                return new DispatcherContinuation(_parentPool);
            }
            public void Push(DispatcherContinuation item)
            {
                _itemContainer.Add(item);
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
