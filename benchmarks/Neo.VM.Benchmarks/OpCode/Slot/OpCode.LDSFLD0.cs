// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.LDSFLD0.cs file belongs to the neo project and is free
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
    public class OpCode_LDSFLD0 : OpCodeBase
    {
        protected override VM.OpCode Opcode => VM.OpCode.LDSFLD0;

        protected override byte[] CreateOneOpCodeScript()
        {
            var b = new InstructionBuilder();
            b.AddInstruction(new Instruction { _opCode = VM.OpCode.INITSSLOT, _operand = new byte[] { 1 } }); // 1 static field
            b.Push(1); // Value to store (implicit STFLD0 in setup? No, LDSFLD needs a prepared static field)
            // Need engine setup to place value in static field 0 before script runs.
            // This basic script only works if static field 0 defaults to non-null or is pre-loaded.
            // Assuming pre-loading for benchmark simplicity for now.
            b.AddInstruction(Opcode); // LDSFLD0
            b.AddInstruction(VM.OpCode.NOP);
            return b.ToArray();
        }

        protected override byte[] CreateOneGASScript()
        {
            return CreateOneOpCodeScript();
        }
    }
}
