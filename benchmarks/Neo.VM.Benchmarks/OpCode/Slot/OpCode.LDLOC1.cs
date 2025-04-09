// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.LDLOC1.cs file belongs to the neo project and is free
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
    public class OpCode_LDLOC1 : OpCodeBase
    {
        protected override VM.OpCode Opcode => VM.OpCode.LDLOC1;

        protected override byte[] CreateOneOpCodeScript()
        {
            var b = new InstructionBuilder();
            b.AddInstruction(new Instruction { _opCode = VM.OpCode.INITSLOT, _operand = new byte[] { 2, 0 } });
            b.Push(1);
            b.AddInstruction(VM.OpCode.STLOC1);
            b.AddInstruction(Opcode);
            b.AddInstruction(VM.OpCode.NOP);
            return b.ToArray();
        }

        protected override byte[] CreateOneGASScript() => CreateOneOpCodeScript();
    }
}
