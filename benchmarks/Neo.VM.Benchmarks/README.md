# Neo.VM Opcode Benchmarks

This project contains benchmarks for the opcodes defined in `src/Neo.VM/OpCode.cs`. The goal is to provide a comprehensive performance analysis of each opcode within the Neo Virtual Machine.

## Goal

To benchmark every opcode available in the Neo VM to understand their individual performance characteristics under various conditions. This helps identify potential bottlenecks and informs optimization efforts.

## Structure

Benchmarks will be organized based on the opcode categories defined in `src/Neo.VM/OpCode.cs`:

-   Constants
-   Flow Control
-   Stack Operations
-   Slot Operations (Static Fields, Local Variables, Arguments)
-   Splice Operations
-   Bitwise Logic
-   Arithmetic Operations
-   Compound-Type Operations
-   Type Operations
-   Extensions

Each category will have a dedicated benchmark class (e.g., `ConstantOpCodeBenchmarks.cs`, `FlowControlOpCodeBenchmarks.cs`).

## Opcodes to Benchmark

All opcodes listed in `src/Neo.VM/OpCode.cs` need benchmark coverage. This includes, but is not limited to:

*   `PUSHINT*`, `PUSHDATA*`, `PUSH*` (Constants)
*   `JMP*`, `CALL*`, `RET`, `TRY*`, `THROW`, `ASSERT` (Flow Control)
*   `DROP`, `DUP`, `OVER`, `SWAP`, `ROT`, `ROLL` (Stack)
*   `LDSFLD*`, `STSFLD*`, `LDLOC*`, `STLOC*`, `LDARG*`, `STARG*` (Slot)
*   `NEWBUFFER`, `MEMCPY`, `CAT`, `SUBSTR` (Splice)
*   `INVERT`, `AND`, `OR`, `XOR`, `EQUAL` (Bitwise)
*   `INC`, `DEC`, `ADD`, `SUB`, `MUL`, `DIV`, `MOD` (Arithmetic)
*   `PACK*`, `UNPACK`, `NEWARRAY*`, `NEWSTRUCT*`, `NEWMAP`, `PICKITEM`, `SETITEM` (Compound)
*   `ISNULL`, `ISTYPE`, `CONVERT` (Types)
*   `ABORTMSG`, `ASSERTMSG` (Extensions)

*(This list is illustrative and should be synchronized with the actual content of `OpCode.cs`)*

## Running Benchmarks

Benchmarks can be run using the standard `dotnet run` command with the appropriate benchmark runner configuration.

```bash
cd benchmarks/Neo.VM.Benchmarks
dotnet run -c Release --filter *
```

## Contribution

Contributions are welcome. Please ensure any new benchmarks follow the established structure and cover their intended opcodes thoroughly. Update this `README.md` if the structure changes or significant new categories are added.
