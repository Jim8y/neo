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
    protected readonly Queue<T> _available = new();
    protected readonly HashSet<T> _inUse = new();
    protected readonly int _maxSize;
    protected readonly int _minSize;

    public LimitedObjectPool(int maxSize, int minSize)
    {
        if (minSize > maxSize)
        {
            throw new ArgumentException("Minimum size cannot be greater than maximum size.");
        }

        _maxSize = maxSize;
        _minSize = minSize;

        // Preallocate the pool with the minimum size
        for (int i = 0; i < _minSize; i++)
        {
            _available.Enqueue(new T());
        }
    }

    public T Get(E value)
    {
        T item;
        if (_available.Count > 0)
        {
            item = _available.Dequeue();
        }
        else
        {
            item = new T();
        }
        if (_inUse.Count < _maxSize)
        {
            item.SetValue(value);
            _inUse.Add(item);
            return item;
        }
        throw new InvalidOperationException("No available objects in the pool.");
    }

    public void Return(T item)
    {
        try
        {
            _inUse.Remove(item);
        }
        catch (Exception e)
        {
            // Can not be removed from _inUse as its not added to the _inUse
        }
        finally
        {
            item.Reset();
            _available.Enqueue(item);
        }
    }
}
