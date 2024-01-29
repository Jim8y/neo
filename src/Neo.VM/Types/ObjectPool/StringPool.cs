// Copyright (C) 2015-2024 The Neo Project.
//
// StringPool.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.IO.ObjectPool;
using System;
using System.Runtime.CompilerServices;

namespace Neo.VM.Types.ObjectPool;

public class StringPool : LimitedObjectPool<ByteString, ReadOnlyMemory<byte>>
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ByteString Get(string value)
    {
        ByteString? item=null;
        if (Available.Count > 0)
        {
            item = Available.Dequeue();
            item.SetValue(value);
        }
        else
        {
            item = new ByteString();
        }
        item.SetValue(value);
        return item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ByteString Get(byte[] value)
    {
        ByteString item;
        if (Available.Count > 0)
        {
            item = Available.Dequeue();
            item.SetValue(value);
        }
        else
        {
            item = new ByteString();
        }
        item.SetValue(value);
        return item;
    }

    public StringPool(uint maxSize) : base(maxSize, maxSize/2)
    {
    }
}
