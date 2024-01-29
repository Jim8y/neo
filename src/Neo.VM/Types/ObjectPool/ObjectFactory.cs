// Copyright (C) 2015-2024 The Neo Project.
//
// ObjectFactory.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Neo.VM.Types.ObjectPool;

public static class ObjectFactory
{
    public static readonly BooleanPool BooleanPool ;
    public static readonly IntegerPool IntegerPool;
    public static readonly StringPool StringPool;

    static ObjectFactory()
    {
        BooleanPool ??= new BooleanPool(ExecutionEngineLimits.Default.MaxStackSize);
        IntegerPool ??= new IntegerPool(ExecutionEngineLimits.Default.MaxStackSize);
        StringPool ??= new StringPool(ExecutionEngineLimits.Default.MaxStackSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Integer Get(BigInteger integer) => IntegerPool.Get(integer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean Get(bool value) => BooleanPool.Get(value);
    //
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static ByteString Get(string value) => StringPool.Get(value);
    //
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static ByteString Get(ReadOnlyMemory<byte> value) => StringPool.Get(value);
    //
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static ByteString Get(byte[] value)
    // {
    //     return StringPool.Get(value);
    // }
}
