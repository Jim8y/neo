// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.CALLT.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using System;
using Neo.VM.Types; // For CallToken, Pointer etc.
using System.Linq; // For First()

namespace Neo.VM.Benchmark.OpCode
{
    // Benchmark for CALLT
    public class OpCode_CALLT : OpCodeBase
    {
        private const short CallTokenId = 1; // Example Token ID

        protected override VM.OpCode Opcode => VM.OpCode.CALLT;

        // Setup: Define a function, assign it a token, then CALLT
        protected override byte[] CreateOneOpCodeScript()
        {
            var builder = new InstructionBuilder();

            // Target function (just RET)
            var retInstruction = builder.AddInstruction(VM.OpCode.RET);

            // Calculate offset after building intermediate script
            var tempScriptForOffset = builder._instructions.ToArray();
            tempScriptForOffset.RebuildOffsets();
            int retOffset = tempScriptForOffset.First(i => i == retInstruction).Offset;

            // Reset builder and build final script
            builder._instructions.Clear();

            // CALLT instruction using the Token ID
            builder.AddInstruction(new Instruction { _opCode = Opcode, _operand = BitConverter.GetBytes(CallTokenId) });

            // Add a NOP after the call for the benchmark engine to stop at
            builder.AddInstruction(VM.OpCode.NOP);

            // Construct the script and set its CallTokens
            var scriptBytes = builder.ToArray();

            // TODO: Verify how BenchmarkEngine handles scripts with tokens.
            // The standard OpCodeBase setup might not work directly with CALLT
            // as it loads scripts purely from byte[]. We might need to override
            // the IterationSetup or use a different base/approach for CALLT.
            // For now, returning the bytes. Execution might fail if engine doesn't load tokens.

            // --- Example of creating Script object (might not be used by current base setup) ---
            // var script = new Script(scriptBytes, true);
            // script.Tokens = new CallToken[]
            // {
            //     new CallToken { Hash = script.ScriptHash, MethodOffset = retOffset, ParametersCount = 0, HasReturnValue = false }
            // };
            // ---------------------------------------------------------------------------------

            return scriptBytes;
        }

        protected override byte[] CreateOneGASScript()
        {
            // TODO: Implement GAS benchmark if needed
            // TODO: Ensure GAS script setup handles tokens correctly
            return CreateOneOpCodeScript(); // Reuse script for now, might be incorrect
        }
    }
}
