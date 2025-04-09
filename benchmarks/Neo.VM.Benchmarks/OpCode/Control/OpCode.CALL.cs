// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.CALL.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

namespace Neo.VM.Benchmark.OpCode
{
    // Corrected implementation for CALL (1-byte offset)
    public class OpCode_CALL : OpCodeBase
    {
        private JumpTarget? _retTarget;

        protected override VM.OpCode Opcode => VM.OpCode.CALL;

        // Setup a simple function (just RET) and call it
        protected override byte[] CreateOneOpCodeScript()
        {
            var builder = new InstructionBuilder();

            // Target function (just RET) - Simplify object init
            _retTarget = new JumpTarget { _instruction = builder.AddInstruction(VM.OpCode.RET) };

            // CALL instruction targeting the RET (using 1-byte relative offset)
            // Note: ScriptBuilder handles offset calculation if target is known.
            builder.Jump(VM.OpCode.CALL, _retTarget);

            // Add a NOP after the call for the benchmark engine to stop at
            builder.AddInstruction(VM.OpCode.NOP);

            return builder.ToArray();
        }

        protected override byte[] CreateOneGASScript()
        {
            // TODO: Implement GAS benchmark if needed, requires careful setup
            // For now, focusing on OpCode benchmark
            return CreateOneOpCodeScript(); // Reuse script for now, Bench_OneGAS will measure CALL + RET
            // throw new NotImplementedException();
        }
    }
}
