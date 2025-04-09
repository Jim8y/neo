// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.POPITEM.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using BenchmarkDotNet.Attributes;

namespace Neo.VM.Benchmark.OpCode
{
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class OpCode_POPITEM
    {
        private BenchmarkEngine? _engine;
        private VM.OpCode Opcode => VM.OpCode.POPITEM;
        private Script? _script;

        // Setup an array with some items to pop
        [GlobalSetup]
        public void Setup()
        {
            var builder = new InstructionBuilder();
            builder.Push(1);
            builder.Push(2);
            builder.Push(3);
            builder.Push(3); // Size of array
            builder.AddInstruction(VM.OpCode.PACK); // Pack into array
            // DUP the array because POPITEM modifies it
            builder.AddInstruction(VM.OpCode.DUP);
            builder.AddInstruction(Opcode); // POPITEM
            _script = builder.ToArray();
            _engine = new BenchmarkEngine();
        }

        [IterationCleanup]
        public void Cleanup()
        {
            _engine?.Dispose();
        }

        [Benchmark]
        public void Bench()
        {
            // Need to reload script each time as POPITEM modifies the array state
            _engine!.LoadScript(_script!);
            _engine.Execute();
        }
    }
}
