// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.STLOC.cs file belongs to the neo project and is free
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
    public class OpCode_STLOC : OpCodeBase
    {
        private const byte LocalIndex = 2; // Use an index > 6

        protected override VM.OpCode Opcode => VM.OpCode.STLOC;

        protected override byte[] CreateOneOpCodeScript()
        {
            var builder = new InstructionBuilder();
            // Need to initialize locals and push value to store
            builder.AddInstruction(new Instruction { _opCode = VM.OpCode.INITSLOT, _operand = new byte[] { LocalIndex + 1, 0 } });
            builder.Push(1); // Use builder.Push helper
            // STLOC instruction with operand
            builder.AddInstruction(new Instruction { _opCode = Opcode, _operand = new byte[] { LocalIndex } });
            builder.AddInstruction(VM.OpCode.NOP); // NOP to stop benchmark engine
            return builder.ToArray();
        }

        protected override byte[] CreateOneGASScript()
        {
            // TODO: Implement accurate GAS measurement if needed
            return CreateOneOpCodeScript(); // Placeholder
        }
    }
}
