// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.CALLA.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.VM.Types; // Required for Pointer
using System; // Required for BitConverter
using System.Linq; // Required for First()

namespace Neo.VM.Benchmark.OpCode
{
    // Corrected implementation for CALLA
    public class OpCode_CALLA : OpCodeBase
    {
        protected override VM.OpCode Opcode => VM.OpCode.CALLA;

        // Setup: Push address of a simple RET function, then CALLA
        protected override byte[] CreateOneOpCodeScript()
        {
            var builder = new InstructionBuilder();

            // Target function (just RET)
            var retInstruction = builder.AddInstruction(VM.OpCode.RET);

            // Need to calculate offset after building intermediate script
            var tempScript = builder._instructions.ToArray(); // Get current instructions
            tempScript.RebuildOffsets(); // Calculate offsets
            int retOffset = tempScript.First(i => i == retInstruction).Offset;

            // Reset builder and build final script
            builder._instructions.Clear();

            // Push the address of the RET function onto the stack using PUSHA
            builder.AddInstruction(new Instruction { _opCode = VM.OpCode.PUSHA, _operand = BitConverter.GetBytes(retOffset) });

            // CALLA instruction (will pop the address from the stack)
            builder.AddInstruction(Opcode);

            // Add a NOP after the call for the benchmark engine to stop at
            builder.AddInstruction(VM.OpCode.NOP);

            return builder.ToArray();
        }

        protected override byte[] CreateOneGASScript()
        {
            // TODO: Implement GAS benchmark if needed
            return CreateOneOpCodeScript(); // Reuse script for now
        }
    }
}
