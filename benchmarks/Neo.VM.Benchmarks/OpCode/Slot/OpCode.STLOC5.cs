// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.STLOC5.cs file belongs to the neo project and is free
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
    public class OpCode_STLOC5 : OpCodeBase
    {
        protected override VM.OpCode Opcode => VM.OpCode.STLOC5;

        protected override byte[] CreateOneOpCodeScript()
        {
            var builder = new InstructionBuilder();
            builder.AddInstruction(new Instruction { _opCode = VM.OpCode.INITSLOT, _operand = new byte[] { 6, 0 } });
            builder.Push(1);
            builder.AddInstruction(Opcode);
            builder.AddInstruction(VM.OpCode.NOP);
            return builder.ToArray();
        }

        protected override byte[] CreateOneGASScript()
        {
            return CreateOneOpCodeScript();
        }
    }
}
