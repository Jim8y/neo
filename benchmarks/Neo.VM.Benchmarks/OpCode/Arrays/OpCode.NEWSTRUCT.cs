// Copyright (C) 2015-2024 The Neo Project.
//
// OpCode.NEWSTRUCT.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

namespace Neo.VM.Benchmark.OpCode;

public class OpCode_NEWSTRUCT : OpCodeBase
{

    protected override VM.OpCode Opcode => VM.OpCode.NEWSTRUCT;

    protected override InstructionBuilder CreateBaseLineScript()
    {
        var builder = new InstructionBuilder();
        builder.Push(ItemCount);
        return builder;
    }

    protected override byte[] CreateOneOpCodeScript(ref InstructionBuilder builder)
    {
        builder.AddInstruction(VM.OpCode.NEWSTRUCT);
        return builder.ToArray();
    }

    protected override byte[] CreateOneGASScript(InstructionBuilder builder)
    {
        throw new NotImplementedException();
    }
}

// | Method          | ItemCount | Mean      | Error     | StdDev    | Median    |
//     |---------------- |---------- |----------:|----------:|----------:|----------:|
//     | Bench_OneOpCode | 1         |  3.924 us | 0.2051 us | 0.5579 us |  3.700 us |
//     | Bench_OneOpCode | 32        |  4.207 us | 0.1504 us | 0.4143 us |  4.100 us |
//     | Bench_OneOpCode | 128       |  6.317 us | 0.2726 us | 0.7555 us |  6.100 us |
//     | Bench_OneOpCode | 1024      | 25.456 us | 0.5446 us | 1.5713 us | 24.900 us |
//     | Bench_OneOpCode | 2040      | 31.177 us | 0.6247 us | 0.7671 us | 31.100 us |
