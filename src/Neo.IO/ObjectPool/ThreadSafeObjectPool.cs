// Copyright (C) 2015-2024 The Neo Project.
//
// ThreadSafeObjectPool.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using System;
using System.Collections.Concurrent;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.IO.ObjectPool
{
    public class ThreadSafeObjectPool<T, E> where T : IPoolable<E>, new()
    {
        private readonly ConcurrentQueue<T> _available = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _poolSizeSemaphore;
        private readonly int _maxSize;

        public ThreadSafeObjectPool(int maxSize)
        {
            _maxSize = maxSize;
            _poolSizeSemaphore = new SemaphoreSlim(_maxSize, _maxSize);
        }

        public async Task<T> GetObjectAsync()
        {
            await _poolSizeSemaphore.WaitAsync();

            if (_available.TryDequeue(out T item))
            {
                return item;
            }

            var newItem = new T();
            return newItem;
        }

        public void ReturnObject(T item)
        {
            _available.Enqueue(item);
            _poolSizeSemaphore.Release();
        }
    }

}
