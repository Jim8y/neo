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
    public static readonly BooleanPool BooleanPool = new(short.MaxValue);
    public static readonly IntegerPool IntegerPool = new(short.MaxValue);
    public static readonly StringPool StringPool = new(short.MaxValue);


    public static Integer Get(BigInteger integer) => IntegerPool.Get(integer);
    public static ByteString Get(ReadOnlyMemory<byte> data) => StringPool.Get(data);
    public static ByteString Get(string str) => StringPool.Get(str);
    public static Boolean Get(bool value) => BooleanPool.Get(value);

}
