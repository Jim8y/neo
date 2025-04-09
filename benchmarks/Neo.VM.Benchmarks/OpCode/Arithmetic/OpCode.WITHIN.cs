// Copyright (C) 2015-2025 The Neo Project.
// OpCode.WITHIN.cs
// ... (header)
using BenchmarkDotNet.Attributes;
using System.Numerics;
namespace Neo.VM.Benchmark.OpCode
{
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class OpCode_WITHIN
    {
        private BenchmarkEngine? _engine;
        private VM.OpCode Opcode => VM.OpCode.WITHIN;
        private Script? _script;
        [ParamsSource(nameof(ValuesX))] public BigInteger _valueX; // Value to check
        [ParamsSource(nameof(ValuesA))] public BigInteger _valueA; // Lower bound
        [ParamsSource(nameof(ValuesB))] public BigInteger _valueB; // Upper bound
        public static IEnumerable<BigInteger> ValuesX => new BigInteger[] { -10, 0, 5, 10, 15, 20 };
        public static IEnumerable<BigInteger> ValuesA => new BigInteger[] { 0 };
        public static IEnumerable<BigInteger> ValuesB => new BigInteger[] { 10 };
        [GlobalSetup]
        public void Setup() { var b = new InstructionBuilder(); b.Push(_valueX); b.Push(_valueA); b.Push(_valueB); b.AddInstruction(Opcode); _script = b.ToArray(); _engine = new BenchmarkEngine(); }
        [IterationCleanup] public void Cleanup() { _engine?.Dispose(); }
        [Benchmark] public void Bench() { _engine!.LoadScript(_script!); _engine.Execute(); }
    }
}
