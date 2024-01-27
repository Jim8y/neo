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

namespace Neo.VM.Types.ObjectPool;

public static class ObjectFactory
{
    public static readonly BooleanPool BooleanPool = new(ExecutionEngineLimits.Default.MaxStackSize);
    public static readonly IntegerPool IntegerPool = new(ExecutionEngineLimits.Default.MaxStackSize);
    public static readonly StringPool StringPool = new(ExecutionEngineLimits.Default.MaxStackSize);

    public static Integer Get(BigInteger integer) => IntegerPool.Get(integer);
    public static Boolean Get(bool value) => BooleanPool.Get(value);
    public static ByteString Get(string value) => StringPool.Get(value);
    public static ByteString Get(ReadOnlyMemory<byte> value) => StringPool.Get(value);
}
