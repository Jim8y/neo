// Copyright (C) 2015-2025 The Neo Project.
// OpCode.NZ.cs
// ... (header)
using BenchmarkDotNet.Attributes;
using System.Numerics;
namespace Neo.VM.Benchmark.OpCode
{
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class OpCode_NZ
    {
        private BenchmarkEngine? _engine;
        private VM.OpCode Opcode => VM.OpCode.NZ;
        private Script? _script;
        [Params(true, false, 0, 1, -1, 100)] public object? _value;
        [GlobalSetup]
        public void Setup() { var b = new InstructionBuilder(); b.Push(_value); b.AddInstruction(Opcode); _script = b.ToArray(); _engine = new BenchmarkEngine(); }
        [IterationCleanup] public void Cleanup() { _engine?.Dispose(); }
        [Benchmark] public void Bench() { _engine!.LoadScript(_script!); _engine.Execute(); }
    }
}
