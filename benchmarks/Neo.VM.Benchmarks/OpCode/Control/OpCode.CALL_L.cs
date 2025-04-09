// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.CALL_L.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

namespace Neo.VM.Benchmark.OpCode
{
    // Benchmark for CALL_L (4-byte offset)
    public class OpCode_CALL_L : OpCodeBase
    {
        private JumpTarget? _retTarget;

        protected override VM.OpCode Opcode => VM.OpCode.CALL_L;

        // Setup a simple function (just RET) and call it
        protected override byte[] CreateOneOpCodeScript()
        {
            var builder = new InstructionBuilder();

            // Target function (just RET) - Simplify object init
            _retTarget = new JumpTarget { _instruction = builder.AddInstruction(VM.OpCode.RET) };

            // CALL_L instruction targeting the RET (using 4-byte relative offset)
            // Note: ScriptBuilder handles offset calculation if target is known.
            builder.Jump(VM.OpCode.CALL_L, _retTarget);

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
