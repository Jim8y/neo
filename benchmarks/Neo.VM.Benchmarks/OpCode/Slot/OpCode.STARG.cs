// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.STARG.cs file belongs to the neo project and is free
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
    public class OpCode_STARG : OpCodeBase
    {
        private const byte ArgIndex = 8; // Use index > 6

        protected override VM.OpCode Opcode => VM.OpCode.STARG;

        protected override byte[] CreateOneOpCodeScript()
        {
            var b = new InstructionBuilder();
            b.AddInstruction(new Instruction { _opCode = VM.OpCode.INITSLOT, _operand = new byte[] { 0, ArgIndex + 1 } }); // Ensure enough args
            b.Push(1); // Value to store
            // Add STARG manually with operand
            b.AddInstruction(new Instruction { _opCode = Opcode, _operand = new byte[] { ArgIndex } });
            b.AddInstruction(VM.OpCode.NOP);
            return b.ToArray();
        }

        protected override byte[] CreateOneGASScript()
        {
            return CreateOneOpCodeScript();
        }
    }
}
