// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.ABS.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using BenchmarkDotNet.Attributes;
using System.Numerics;

namespace Neo.VM.Benchmark.OpCode
{
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class OpCode_ABS
    {
        private BenchmarkEngine? _engine;
        private VM.OpCode Opcode => VM.OpCode.ABS;
        private Script? _script;

        [ParamsSource(nameof(Values))]
        public BigInteger _value;

        public static IEnumerable<BigInteger> Values => OpCode_SIGN.Values; // Reuse values from SIGN

        [GlobalSetup]
        public void Setup()
        {
            var builder = new InstructionBuilder();
            builder.Push(_value);
            builder.AddInstruction(Opcode);
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
            _engine!.LoadScript(_script!);
            _engine.Execute();
        }
    }
}
