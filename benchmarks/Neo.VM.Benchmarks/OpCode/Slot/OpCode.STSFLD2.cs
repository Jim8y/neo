// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.STSFLD2.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.
using System;
namespace Neo.VM.Benchmark.OpCode
{
    public class OpCode_STSFLD2 : OpCodeBase
    {
        protected override VM.OpCode Opcode => VM.OpCode.STSFLD2;

        protected override byte[] CreateOneOpCodeScript()
        {
            var b = new InstructionBuilder();
            var builder = new InstructionBuilder();
            builder.AddInstruction(Opcode);
            return builder.ToArray();
        }

        protected override byte[] CreateOneGASScript()
        {
            throw new NotImplementedException();
        }
    }
}
