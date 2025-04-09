// Copyright (C) 2015-2025 The Neo Project.
//
// OpCode.PUSHT.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using BenchmarkDotNet.Attributes;

namespace Neo.VM.Benchmark.OpCode.Push
{
    // Using a simpler structure without OpCodeBase for single, simple opcodes
    // as seen in OpCode.PUSHNULL.cs pattern
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class OpCode_PUSHT
    {
        private BenchmarkEngine? _engine;
        private Script? _script;

        [GlobalSetup]
        public void Setup()
        {
            _script = new ScriptBuilder().Emit(VM.OpCode.PUSHT).ToArray();
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
