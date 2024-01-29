// Copyright (C) 2015-2024 The Neo Project.
//
// LimitedObjectPool.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using System;

namespace Neo.IO.ObjectPool;

using System.Collections.Generic;

public class LimitedObjectPool<T, E> where T : IPoolable<E>, new()
{
    protected readonly Queue<T> Available = new();
    // protected readonly HashSet<T> InUse = new();
    protected readonly uint MaxSize;
    protected readonly uint MinSize;

    public LimitedObjectPool(uint maxSize, uint minSize)
    {
        if (minSize > maxSize)
        {
            throw new ArgumentException("Minimum size cannot be greater than maximum size.");
        }

        MaxSize = maxSize;
        MinSize = minSize;

        // Preallocate the pool with the minimum size
        for (var i = 0; i < MinSize; i++)
        {
            Available.Enqueue(new T());
        }
    }

    public T Get(E value)
    {
        T item;
        if (Available.Count > 0)
        {
            item = Available.Dequeue();
        }
        else
        {
            item = new T();
        }
        item.SetValue(value);
        return item;
    }

    public void Return(T item)
    {
        if (Available.Count >= MaxSize) return;
        Available.Enqueue(item);
    }
}
