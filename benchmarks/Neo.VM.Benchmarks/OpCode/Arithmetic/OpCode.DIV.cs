// Copyright (C) 2015-2025 The Neo Project.
// OpCode.DIV.cs
// ... (header)
using BenchmarkDotNet.Attributes;
using System.Numerics;
namespace Neo.VM.Benchmark.OpCode
{
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class OpCode_DIV
    {
        private BenchmarkEngine? _engine;
        private VM.OpCode Opcode => VM.OpCode.DIV;
        private Script? _script;
        [ParamsSource(nameof(ValuesA))] public BigInteger _valueA;
        [ParamsSource(nameof(ValuesB))] public BigInteger _valueB;
        // Filter ValuesB to exclude zero to prevent DivideByZeroException
        public static IEnumerable<BigInteger> ValuesA => OpCode_SIGN.Values;
        public static IEnumerable<BigInteger> ValuesB => OpCode_SIGN.Values.Where(v => v != BigInteger.Zero);
        [GlobalSetup]
        public void Setup() { var b = new InstructionBuilder(); b.Push(_valueA); b.Push(_valueB); b.AddInstruction(Opcode); _script = b.ToArray(); _engine = new BenchmarkEngine(); }
        [IterationCleanup] public void Cleanup() { _engine?.Dispose(); }
        [Benchmark] public void Bench() { _engine!.LoadScript(_script!); _engine.Execute(); }
    }
}
