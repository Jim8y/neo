// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.DROP.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

namespace Neo.VM.Benchmark.OpCode
{
    public class OpCode_DROP : OpCodeBase
    {
        protected override VM.OpCode Opcode => VM.OpCode.DROP;

        protected override byte[] CreateOneOpCodeScript()
        {
            var builder = new InstructionBuilder();
            var initBegin = new JumpTarget();
            // Initialize 1 local slot
            builder.AddInstruction(new Instruction { _opCode = VM.OpCode.INITSLOT, _operand = new byte[] { 1, 0 } });
            // Store ItemCount in local 0
            builder.Push(ItemCount);
            builder.AddInstruction(VM.OpCode.STLOC0);
            // Loop start NOP
            initBegin._instruction = builder.AddInstruction(VM.OpCode.NOP);
            // Push a large item (buffer) onto the stack
            builder.Push(ushort.MaxValue * 2);
            builder.AddInstruction(VM.OpCode.NEWBUFFER);
            // builder.Push(0); // Original comment
            // Load, decrement, and store loop counter (local 0)
            builder.AddInstruction(VM.OpCode.LDLOC0);
            builder.AddInstruction(VM.OpCode.DEC);
            builder.AddInstruction(VM.OpCode.STLOC0);
            // Load counter again for jump condition
            builder.AddInstruction(VM.OpCode.LDLOC0);
            // Jump back if counter is non-zero
            builder.Jump(VM.OpCode.JMPIF, initBegin);
            // Push ItemCount (N) for PACK
            builder.Push(ItemCount);
            // Pack the N buffers into an array
            builder.AddInstruction(VM.OpCode.PACK);
            // Benchmark the DROP opcode on the created array
            builder.AddInstruction(Opcode);
            builder.AddInstruction(VM.OpCode.NOP); // Added NOP for engine stop
            return builder.ToArray();
        }

        protected override byte[] CreateOneGASScript()
        {
            var builder = new InstructionBuilder();
            var initBegin = new JumpTarget();
            // Initialize 1 local slot
            builder.AddInstruction(new Instruction { _opCode = VM.OpCode.INITSLOT, _operand = new byte[] { 1, 0 } });
            // Outer loop for GAS measurement
            var loopBegin = new JumpTarget { _instruction = builder.AddInstruction(VM.OpCode.NOP) };
            // Store ItemCount in local 0
            builder.Push(ItemCount);
            builder.AddInstruction(VM.OpCode.STLOC0);
            // Inner loop start NOP (Push N items)
            initBegin._instruction = builder.AddInstruction(VM.OpCode.NOP);
            // Push a large item (buffer) onto the stack
            builder.Push(ushort.MaxValue * 2);
            builder.AddInstruction(VM.OpCode.NEWBUFFER);
            // builder.Push(0); // Original comment
            // Load, decrement, and store loop counter (local 0)
            builder.AddInstruction(VM.OpCode.LDLOC0);
            builder.AddInstruction(VM.OpCode.DEC);
            builder.AddInstruction(VM.OpCode.STLOC0);
            // Load counter again for jump condition
            builder.AddInstruction(VM.OpCode.LDLOC0);
            // Jump back if counter is non-zero (inner loop)
            builder.Jump(VM.OpCode.JMPIF, initBegin);
            // Push ItemCount (N) for PACK
            builder.Push(ItemCount);
            // Pack the N buffers into an array
            builder.AddInstruction(VM.OpCode.PACK);
            // Benchmark the DROP opcode on the created array
            builder.AddInstruction(Opcode);
            // Jump back to outer loop start
            builder.Jump(VM.OpCode.JMP, loopBegin);
            return builder.ToArray();
        }
    }
}

// Old benchmark results removed.
