// Copyright (C) 2015-2024 The Neo Project.
//
// ExecutionEngine.Instruction.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.VM.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Buffer = Neo.VM.Types.Buffer;
using VMArray = Neo.VM.Types.Array;
namespace Neo.VM
{
    partial class ExecutionEngine
    {
        private void ExecuteInstruction(Instruction instruction)
        {
            switch (instruction.OpCode)
            {
                //Push
                case OpCode.PUSHINT8:
                case OpCode.PUSHINT16:
                case OpCode.PUSHINT32:
                case OpCode.PUSHINT64:
                case OpCode.PUSHINT128:
                case OpCode.PUSHINT256:
                    {
                        PushInteger(new BigInteger(instruction.Operand.Span));
                        break;
                    }
                case OpCode.PUSHT:
                    {
                        PushBoolean(true);
                        break;
                    }
                case OpCode.PUSHF:
                    {
                        PushBoolean(false);
                        break;
                    }
                case OpCode.PUSHA:
                    {
                        int position = checked(_currentContext!.InstructionPointer + instruction.TokenI32);
                        if (position < 0 || position > _currentContext.ScriptLength)
                            throw new InvalidOperationException($"Bad pointer address: {position}");
                        Push(new Pointer(_currentContext.Script, position));
                        break;
                    }
                case OpCode.PUSHNULL:
                    {
                        Push(StackItem.Null);
                        break;
                    }
                case OpCode.PUSHDATA1:
                case OpCode.PUSHDATA2:
                case OpCode.PUSHDATA4:
                    {
                        Limits.AssertMaxItemSize(instruction.Operand.Length);
                        Push(instruction.Operand);
                        break;
                    }
                case OpCode.PUSHM1:
                case OpCode.PUSH0:
                case OpCode.PUSH1:
                case OpCode.PUSH2:
                case OpCode.PUSH3:
                case OpCode.PUSH4:
                case OpCode.PUSH5:
                case OpCode.PUSH6:
                case OpCode.PUSH7:
                case OpCode.PUSH8:
                case OpCode.PUSH9:
                case OpCode.PUSH10:
                case OpCode.PUSH11:
                case OpCode.PUSH12:
                case OpCode.PUSH13:
                case OpCode.PUSH14:
                case OpCode.PUSH15:
                case OpCode.PUSH16:
                    {
                        Push((int)instruction.OpCode - (int)OpCode.PUSH0);
                        break;
                    }

                // Control
                case OpCode.NOP: break;
                case OpCode.JMP:
                    {
                        ExecuteJumpOffset(instruction.TokenI8);
                        break;
                    }
                case OpCode.JMP_L:
                    {
                        ExecuteJumpOffset(instruction.TokenI32);
                        break;
                    }
                case OpCode.JMPIF:
                    {
                        if (Pop().ReUse().GetBoolean())
                            ExecuteJumpOffset(instruction.TokenI8);
                        break;
                    }
                case OpCode.JMPIF_L:
                    {
                        if (Pop().ReUse().GetBoolean())
                            ExecuteJumpOffset(instruction.TokenI32);
                        break;
                    }
                case OpCode.JMPIFNOT:
                    {
                        if (!Pop().ReUse().GetBoolean())
                            ExecuteJumpOffset(instruction.TokenI8);
                        break;
                    }
                case OpCode.JMPIFNOT_L:
                    {
                        if (!Pop().ReUse().GetBoolean())
                            ExecuteJumpOffset(instruction.TokenI32);
                        break;
                    }
                case OpCode.JMPEQ:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 == x2)
                            ExecuteJumpOffset(instruction.TokenI8);
                        break;
                    }
                case OpCode.JMPEQ_L:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 == x2)
                            ExecuteJumpOffset(instruction.TokenI32);
                        break;
                    }
                case OpCode.JMPNE:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 != x2)
                            ExecuteJumpOffset(instruction.TokenI8);
                        break;
                    }
                case OpCode.JMPNE_L:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 != x2)
                            ExecuteJumpOffset(instruction.TokenI32);
                        break;
                    }
                case OpCode.JMPGT:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 > x2)
                            ExecuteJumpOffset(instruction.TokenI8);
                        break;
                    }
                case OpCode.JMPGT_L:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 > x2)
                            ExecuteJumpOffset(instruction.TokenI32);
                        break;
                    }
                case OpCode.JMPGE:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 >= x2)
                            ExecuteJumpOffset(instruction.TokenI8);
                        break;
                    }
                case OpCode.JMPGE_L:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 >= x2)
                            ExecuteJumpOffset(instruction.TokenI32);
                        break;
                    }
                case OpCode.JMPLT:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 < x2)
                            ExecuteJumpOffset(instruction.TokenI8);
                        break;
                    }
                case OpCode.JMPLT_L:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 < x2)
                            ExecuteJumpOffset(instruction.TokenI32);
                        break;
                    }
                case OpCode.JMPLE:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 <= x2)
                            ExecuteJumpOffset(instruction.TokenI8);
                        break;
                    }
                case OpCode.JMPLE_L:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        if (x1 <= x2)
                            ExecuteJumpOffset(instruction.TokenI32);
                        break;
                    }
                case OpCode.CALL:
                    {
                        ExecuteCall(checked(_currentContext!.InstructionPointer + instruction.TokenI8));
                        break;
                    }
                case OpCode.CALL_L:
                    {
                        ExecuteCall(checked(_currentContext!.InstructionPointer + instruction.TokenI32));
                        break;
                    }
                case OpCode.CALLA:
                    {
                        var x = Pop<Pointer>();
                        if (x.Script != _currentContext!.Script)
                            throw new InvalidOperationException("Pointers can't be shared between scripts");
                        ExecuteCall(x.Position);
                        break;
                    }
                case OpCode.CALLT:
                    {
                        LoadToken(instruction.TokenU16);
                        break;
                    }
                case OpCode.ABORT:
                    {
                        throw new Exception($"{OpCode.ABORT} is executed.");
                    }
                case OpCode.ASSERT:
                    {
                        var x = Pop().ReUse().GetBoolean();
                        if (!x)
                            throw new Exception($"{OpCode.ASSERT} is executed with false result.");
                        break;
                    }
                case OpCode.THROW:
                    {
                        ExecuteThrow(Pop());
                        break;
                    }
                case OpCode.TRY:
                    {
                        int catchOffset = instruction.TokenI8;
                        int finallyOffset = instruction.TokenI8_1;
                        ExecuteTry(catchOffset, finallyOffset);
                        break;
                    }
                case OpCode.TRY_L:
                    {
                        int catchOffset = instruction.TokenI32;
                        int finallyOffset = instruction.TokenI32_1;
                        ExecuteTry(catchOffset, finallyOffset);
                        break;
                    }
                case OpCode.ENDTRY:
                    {
                        int endOffset = instruction.TokenI8;
                        ExecuteEndTry(endOffset);
                        break;
                    }
                case OpCode.ENDTRY_L:
                    {
                        int endOffset = instruction.TokenI32;
                        ExecuteEndTry(endOffset);
                        break;
                    }
                case OpCode.ENDFINALLY:
                    {
                        if (_currentContext!.TryStack is null)
                            throw new InvalidOperationException($"The corresponding TRY block cannot be found.");
                        if (!_currentContext.TryStack.TryPop(out ExceptionHandlingContext? currentTry))
                            throw new InvalidOperationException($"The corresponding TRY block cannot be found.");

                        if (UncaughtException is null)
                            _currentContext.InstructionPointer = currentTry.EndPointer;
                        else
                            HandleException();

                        isJumping = true;
                        break;
                    }
                case OpCode.RET:
                    {
                        ExecutionContext context_pop = InvocationStack.Pop();
                        EvaluationStack stack_eval = InvocationStack.Count == 0 ? ResultStack : InvocationStack.Peek().EvaluationStack;
                        if (context_pop.EvaluationStack != stack_eval)
                        {
                            if (context_pop.RVCount >= 0 && context_pop.EvaluationStack.Count != context_pop.RVCount)
                                throw new InvalidOperationException("RVCount doesn't match with EvaluationStack");
                            context_pop.EvaluationStack.CopyTo(stack_eval);
                        }
                        if (InvocationStack.Count == 0)
                            State = VMState.HALT;
                        ContextUnloaded(context_pop);
                        isJumping = true;
                        break;
                    }
                case OpCode.SYSCALL:
                    {
                        OnSysCall(instruction.TokenU32);
                        break;
                    }

                // Stack ops
                case OpCode.DEPTH:
                    {
                        PushInteger(_currentContext!.EvaluationStack.Count);
                        break;
                    }
                case OpCode.DROP:
                    {
                        Pop();
                        break;
                    }
                case OpCode.NIP:
                    {
                        _currentContext!.EvaluationStack.Remove<StackItem>(1);
                        break;
                    }
                case OpCode.XDROP:
                    {
                        int n = (int)Pop().ReUse().GetInteger();
                        if (n < 0)
                            throw new InvalidOperationException($"The negative value {n} is invalid for OpCode.{instruction.OpCode}.");
                        _currentContext!.EvaluationStack.Remove<StackItem>(n);
                        break;
                    }
                case OpCode.CLEAR:
                    {
                        _currentContext!.EvaluationStack.Clear();
                        break;
                    }
                case OpCode.DUP:
                    {
                        Push(Peek());
                        break;
                    }
                case OpCode.OVER:
                    {
                        Push(Peek(1));
                        break;
                    }
                case OpCode.PICK:
                    {
                        int n = (int)Pop().ReUse().GetInteger();
                        if (n < 0)
                            throw new InvalidOperationException($"The negative value {n} is invalid for OpCode.{instruction.OpCode}.");
                        Push(Peek(n));
                        break;
                    }
                case OpCode.TUCK:
                    {
                        _currentContext!.EvaluationStack.Insert(2, Peek());
                        break;
                    }
                case OpCode.SWAP:
                    {
                        var x = _currentContext!.EvaluationStack.Remove<StackItem>(1);
                        Push(x);
                        break;
                    }
                case OpCode.ROT:
                    {
                        var x = _currentContext!.EvaluationStack.Remove<StackItem>(2);
                        Push(x);
                        break;
                    }
                case OpCode.ROLL:
                    {
                        int n = (int)Pop().ReUse().GetInteger();
                        if (n < 0)
                            throw new InvalidOperationException($"The negative value {n} is invalid for OpCode.{instruction.OpCode}.");
                        if (n == 0) break;
                        var x = _currentContext!.EvaluationStack.Remove<StackItem>(n);
                        Push(x);
                        break;
                    }
                case OpCode.REVERSE3:
                    {
                        _currentContext!.EvaluationStack.Reverse(3);
                        break;
                    }
                case OpCode.REVERSE4:
                    {
                        _currentContext!.EvaluationStack.Reverse(4);
                        break;
                    }
                case OpCode.REVERSEN:
                    {
                        int n = (int)Pop().ReUse().GetInteger();
                        _currentContext!.EvaluationStack.Reverse(n);
                        break;
                    }

                //Slot
                case OpCode.INITSSLOT:
                    {
                        if (_currentContext!.StaticFields != null)
                            throw new InvalidOperationException($"{instruction.OpCode} cannot be executed twice.");
                        if (instruction.TokenU8 == 0)
                            throw new InvalidOperationException($"The operand {instruction.TokenU8} is invalid for OpCode.{instruction.OpCode}.");
                        _currentContext.StaticFields = new Slot(instruction.TokenU8, ReferenceCounter);
                        break;
                    }
                case OpCode.INITSLOT:
                    {
                        if (_currentContext!.LocalVariables != null || _currentContext.Arguments != null)
                            throw new InvalidOperationException($"{instruction.OpCode} cannot be executed twice.");
                        if (instruction.TokenU16 == 0)
                            throw new InvalidOperationException($"The operand {instruction.TokenU16} is invalid for OpCode.{instruction.OpCode}.");
                        if (instruction.TokenU8 > 0)
                        {
                            _currentContext.LocalVariables = new Slot(instruction.TokenU8, ReferenceCounter);
                        }
                        if (instruction.TokenU8_1 > 0)
                        {
                            StackItem[] items = new StackItem[instruction.TokenU8_1];
                            for (int i = 0; i < instruction.TokenU8_1; i++)
                            {
                                items[i] = Pop();
                            }
                            _currentContext.Arguments = new Slot(items, ReferenceCounter);
                        }
                        break;
                    }
                case OpCode.LDSFLD0:
                case OpCode.LDSFLD1:
                case OpCode.LDSFLD2:
                case OpCode.LDSFLD3:
                case OpCode.LDSFLD4:
                case OpCode.LDSFLD5:
                case OpCode.LDSFLD6:
                    {
                        ExecuteLoadFromSlot(_currentContext!.StaticFields, instruction.OpCode - OpCode.LDSFLD0);
                        break;
                    }
                case OpCode.LDSFLD:
                    {
                        ExecuteLoadFromSlot(_currentContext!.StaticFields, instruction.TokenU8);
                        break;
                    }
                case OpCode.STSFLD0:
                case OpCode.STSFLD1:
                case OpCode.STSFLD2:
                case OpCode.STSFLD3:
                case OpCode.STSFLD4:
                case OpCode.STSFLD5:
                case OpCode.STSFLD6:
                    {
                        ExecuteStoreToSlot(_currentContext!.StaticFields, instruction.OpCode - OpCode.STSFLD0);
                        break;
                    }
                case OpCode.STSFLD:
                    {
                        ExecuteStoreToSlot(_currentContext!.StaticFields, instruction.TokenU8);
                        break;
                    }
                case OpCode.LDLOC0:
                case OpCode.LDLOC1:
                case OpCode.LDLOC2:
                case OpCode.LDLOC3:
                case OpCode.LDLOC4:
                case OpCode.LDLOC5:
                case OpCode.LDLOC6:
                    {
                        ExecuteLoadFromSlot(_currentContext!.LocalVariables, instruction.OpCode - OpCode.LDLOC0);
                        break;
                    }
                case OpCode.LDLOC:
                    {
                        ExecuteLoadFromSlot(_currentContext!.LocalVariables, instruction.TokenU8);
                        break;
                    }
                case OpCode.STLOC0:
                case OpCode.STLOC1:
                case OpCode.STLOC2:
                case OpCode.STLOC3:
                case OpCode.STLOC4:
                case OpCode.STLOC5:
                case OpCode.STLOC6:
                    {
                        ExecuteStoreToSlot(_currentContext!.LocalVariables, instruction.OpCode - OpCode.STLOC0);
                        break;
                    }
                case OpCode.STLOC:
                    {
                        ExecuteStoreToSlot(_currentContext!.LocalVariables, instruction.TokenU8);
                        break;
                    }
                case OpCode.LDARG0:
                case OpCode.LDARG1:
                case OpCode.LDARG2:
                case OpCode.LDARG3:
                case OpCode.LDARG4:
                case OpCode.LDARG5:
                case OpCode.LDARG6:
                    {
                        ExecuteLoadFromSlot(_currentContext!.Arguments, instruction.OpCode - OpCode.LDARG0);
                        break;
                    }
                case OpCode.LDARG:
                    {
                        ExecuteLoadFromSlot(_currentContext!.Arguments, instruction.TokenU8);
                        break;
                    }
                case OpCode.STARG0:
                case OpCode.STARG1:
                case OpCode.STARG2:
                case OpCode.STARG3:
                case OpCode.STARG4:
                case OpCode.STARG5:
                case OpCode.STARG6:
                    {
                        ExecuteStoreToSlot(_currentContext!.Arguments, instruction.OpCode - OpCode.STARG0);
                        break;
                    }
                case OpCode.STARG:
                    {
                        ExecuteStoreToSlot(_currentContext!.Arguments, instruction.TokenU8);
                        break;
                    }

                // Splice
                case OpCode.NEWBUFFER:
                    {
                        int length = (int)Pop().ReUse().GetInteger();
                        Limits.AssertMaxItemSize(length);
                        Push(new Buffer(length));
                        break;
                    }
                case OpCode.MEMCPY:
                    {
                        int count = (int)Pop().ReUse().GetInteger();
                        if (count < 0)
                            throw new InvalidOperationException($"The value {count} is out of range.");
                        int si = (int)Pop().ReUse().GetInteger();
                        if (si < 0)
                            throw new InvalidOperationException($"The value {si} is out of range.");
                        ReadOnlySpan<byte> src = Pop().ReUse().GetSpan();
                        if (checked(si + count) > src.Length)
                            throw new InvalidOperationException($"The value {count} is out of range.");
                        int di = (int)Pop().ReUse().GetInteger();
                        if (di < 0)
                            throw new InvalidOperationException($"The value {di} is out of range.");
                        Buffer dst = Pop<Buffer>();
                        if (checked(di + count) > dst.Size)
                            throw new InvalidOperationException($"The value {count} is out of range.");
                        src.Slice(si, count).CopyTo(dst.InnerBuffer.Span[di..]);
                        break;
                    }
                case OpCode.CAT:
                    {
                        var x2 = Pop().ReUse().GetSpan();
                        var x1 = Pop().ReUse().GetSpan();
                        int length = x1.Length + x2.Length;
                        Limits.AssertMaxItemSize(length);
                        Buffer result = new(length, false);
                        x1.CopyTo(result.InnerBuffer.Span);
                        x2.CopyTo(result.InnerBuffer.Span[x1.Length..]);
                        Push(result);
                        break;
                    }
                case OpCode.SUBSTR:
                    {
                        int count = (int)Pop().ReUse().GetInteger();
                        if (count < 0)
                            throw new InvalidOperationException($"The value {count} is out of range.");
                        int index = (int)Pop().ReUse().GetInteger();
                        if (index < 0)
                            throw new InvalidOperationException($"The value {index} is out of range.");
                        var x = Pop().ReUse().GetSpan();
                        if (index + count > x.Length)
                            throw new InvalidOperationException($"The value {count} is out of range.");
                        Buffer result = new(count, false);
                        x.Slice(index, count).CopyTo(result.InnerBuffer.Span);
                        Push(result);
                        break;
                    }
                case OpCode.LEFT:
                    {
                        int count = (int)Pop().ReUse().GetInteger();
                        if (count < 0)
                            throw new InvalidOperationException($"The value {count} is out of range.");
                        var x = Pop().ReUse().GetSpan();
                        if (count > x.Length)
                            throw new InvalidOperationException($"The value {count} is out of range.");
                        Buffer result = new(count, false);
                        x[..count].CopyTo(result.InnerBuffer.Span);
                        Push(result);
                        break;
                    }
                case OpCode.RIGHT:
                    {
                        int count = (int)Pop().ReUse().GetInteger();
                        if (count < 0)
                            throw new InvalidOperationException($"The value {count} is out of range.");
                        var x = Pop().ReUse().GetSpan();
                        if (count > x.Length)
                            throw new InvalidOperationException($"The value {count} is out of range.");
                        Buffer result = new(count, false);
                        x[^count..^0].CopyTo(result.InnerBuffer.Span);
                        Push(result);
                        break;
                    }

                // Bitwise logic
                case OpCode.INVERT:
                    {
                        var x = Pop().ReUse().GetInteger();
                        PushInteger(~x);
                        break;
                    }
                case OpCode.AND:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(x1 & x2);
                        break;
                    }
                case OpCode.OR:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(x1 | x2);
                        break;
                    }
                case OpCode.XOR:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(x1 ^ x2);
                        break;
                    }
                case OpCode.EQUAL:
                    {
                        StackItem x2 = Pop();
                        StackItem x1 = Pop();
                        PushBoolean(x1.Equals(x2, Limits));
                        break;
                    }
                case OpCode.NOTEQUAL:
                    {
                        StackItem x2 = Pop();
                        StackItem x1 = Pop();
                        PushBoolean(!x1.Equals(x2, Limits));
                        break;
                    }

                // Numeric
                case OpCode.SIGN:
                    {
                        var x = Pop().ReUse().GetInteger();
                        PushInteger(x.Sign);
                        break;
                    }
                case OpCode.ABS:
                    {
                        var x = Pop().ReUse().GetInteger();
                        PushInteger(BigInteger.Abs(x));
                        break;
                    }
                case OpCode.NEGATE:
                    {
                        var x = Pop().ReUse().GetInteger();
                        PushInteger(-x);
                        break;
                    }
                case OpCode.INC:
                    {
                        var x = Pop().ReUse().GetInteger();
                        PushInteger(x + 1);
                        break;
                    }
                case OpCode.DEC:
                    {
                        var x = Pop().ReUse().GetInteger();
                        PushInteger(x - 1);
                        break;
                    }
                case OpCode.ADD:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(x1 + x2);
                        break;
                    }
                case OpCode.SUB:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(x1 - x2);
                        break;
                    }
                case OpCode.MUL:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(x1 * x2);
                        break;
                    }
                case OpCode.DIV:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(x1 / x2);
                        break;
                    }
                case OpCode.MOD:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(x1 % x2);
                        break;
                    }
                case OpCode.POW:
                    {
                        var exponent = (int)Pop().ReUse().GetInteger();
                        Limits.AssertShift(exponent);
                        var value = Pop().ReUse().GetInteger();
                        PushInteger(BigInteger.Pow(value, exponent));
                        break;
                    }
                case OpCode.SQRT:
                    {
                        PushInteger(Pop().ReUse().GetInteger().Sqrt());
                        break;
                    }
                case OpCode.MODMUL:
                    {
                        var modulus = Pop().ReUse().GetInteger();
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(x1 * x2 % modulus);
                        break;
                    }
                case OpCode.MODPOW:
                    {
                        var modulus = Pop().ReUse().GetInteger();
                        var exponent = Pop().ReUse().GetInteger();
                        var value = Pop().ReUse().GetInteger();
                        var result = exponent == -1
                            ? value.ModInverse(modulus)
                            : BigInteger.ModPow(value, exponent, modulus);
                        PushInteger(result);
                        break;
                    }
                case OpCode.SHL:
                    {
                        int shift = (int)Pop().ReUse().GetInteger();
                        Limits.AssertShift(shift);
                        if (shift == 0) break;
                        var x = Pop().ReUse().GetInteger();
                        PushInteger(x << shift);
                        break;
                    }
                case OpCode.SHR:
                    {
                        int shift = (int)Pop().ReUse().GetInteger();
                        Limits.AssertShift(shift);
                        if (shift == 0) break;
                        var x = Pop().ReUse().GetInteger();
                        PushInteger(x >> shift);
                        break;
                    }
                case OpCode.NOT:
                    {
                        var x = Pop().ReUse().GetBoolean();
                        PushBoolean(!x);
                        break;
                    }
                case OpCode.BOOLAND:
                    {
                        var x2 = Pop().ReUse().GetBoolean();
                        var x1 = Pop().ReUse().GetBoolean();
                        PushBoolean(x1 && x2);
                        break;
                    }
                case OpCode.BOOLOR:
                    {
                        var x2 = Pop().ReUse().GetBoolean();
                        var x1 = Pop().ReUse().GetBoolean();
                        PushBoolean(x1 || x2);
                        break;
                    }
                case OpCode.NZ:
                    {
                        var x = Pop().ReUse().GetInteger();
                        PushBoolean(!x.IsZero);
                        break;
                    }
                case OpCode.NUMEQUAL:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushBoolean(x1 == x2);
                        break;
                    }
                case OpCode.NUMNOTEQUAL:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushBoolean(x1 != x2);
                        break;
                    }
                case OpCode.LT:
                    {
                        var x2 = Pop();
                        var x1 = Pop();
                        if (x1.IsNull || x2.IsNull)
                            PushBoolean(false);
                        else
                            PushBoolean(x1.GetInteger() < x2.GetInteger());
                        break;
                    }
                case OpCode.LE:
                    {
                        var x2 = Pop();
                        var x1 = Pop();
                        if (x1.IsNull || x2.IsNull)
                            PushBoolean(false);
                        else
                            PushBoolean(x1.GetInteger() <= x2.GetInteger());
                        break;
                    }
                case OpCode.GT:
                    {
                        var x2 = Pop();
                        var x1 = Pop();
                        if (x1.IsNull || x2.IsNull)
                            PushBoolean(false);
                        else
                            PushBoolean(x1.GetInteger() > x2.GetInteger());
                        break;
                    }
                case OpCode.GE:
                    {
                        var x2 = Pop();
                        var x1 = Pop();
                        if (x1.IsNull || x2.IsNull)
                            PushBoolean(false);
                        else
                            PushBoolean(x1.GetInteger() >= x2.GetInteger());
                        break;
                    }
                case OpCode.MIN:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(BigInteger.Min(x1, x2));
                        break;
                    }
                case OpCode.MAX:
                    {
                        var x2 = Pop().ReUse().GetInteger();
                        var x1 = Pop().ReUse().GetInteger();
                        PushInteger(BigInteger.Max(x1, x2));
                        break;
                    }
                case OpCode.WITHIN:
                    {
                        BigInteger b = Pop().ReUse().GetInteger();
                        BigInteger a = Pop().ReUse().GetInteger();
                        var x = Pop().ReUse().GetInteger();
                        PushBoolean(a <= x && x < b);
                        break;
                    }

                // Compound-type
                case OpCode.PACKMAP:
                    {
                        int size = (int)Pop().ReUse().GetInteger();
                        if (size < 0 || size * 2 > _currentContext!.EvaluationStack.Count)
                            throw new InvalidOperationException($"The value {size} is out of range.");
                        Map map = new(ReferenceCounter);
                        for (int i = 0; i < size; i++)
                        {
                            PrimitiveType key = Pop<PrimitiveType>();
                            StackItem value = Pop();
                            map[key] = value;
                        }
                        Push(map);
                        break;
                    }
                case OpCode.PACKSTRUCT:
                    {
                        int size = (int)Pop().ReUse().GetInteger();
                        if (size < 0 || size > _currentContext!.EvaluationStack.Count)
                            throw new InvalidOperationException($"The value {size} is out of range.");
                        Struct @struct = new(ReferenceCounter);
                        for (int i = 0; i < size; i++)
                        {
                            StackItem item = Pop();
                            @struct.Add(item);
                        }
                        Push(@struct);
                        break;
                    }
                case OpCode.PACK:
                    {
                        int size = (int)Pop().ReUse().GetInteger();
                        if (size < 0 || size > _currentContext!.EvaluationStack.Count)
                            throw new InvalidOperationException($"The value {size} is out of range.");
                        VMArray array = new(ReferenceCounter);
                        for (int i = 0; i < size; i++)
                        {
                            StackItem item = Pop();
                            array.Add(item);
                        }
                        Push(array);
                        break;
                    }
                case OpCode.UNPACK:
                    {
                        CompoundType compound = Pop<CompoundType>();
                        switch (compound)
                        {
                            case Map map:
                                foreach (var (key, value) in map.Reverse())
                                {
                                    Push(value);
                                    Push(key);
                                }
                                break;
                            case VMArray array:
                                for (int i = array.Count - 1; i >= 0; i--)
                                {
                                    Push(array[i]);
                                }
                                break;
                            default:
                                throw new InvalidOperationException($"Invalid type for {instruction.OpCode}: {compound.Type}");
                        }
                        PushInteger(compound.Count);
                        break;
                    }
                case OpCode.NEWARRAY0:
                    {
                        Push(new VMArray(ReferenceCounter));
                        break;
                    }
                case OpCode.NEWARRAY:
                case OpCode.NEWARRAY_T:
                    {
                        int n = (int)Pop().ReUse().GetInteger();
                        if (n < 0 || n > Limits.MaxStackSize)
                            throw new InvalidOperationException($"MaxStackSize exceed: {n}");
                        StackItem item;
                        if (instruction.OpCode == OpCode.NEWARRAY_T)
                        {
                            StackItemType type = (StackItemType)instruction.TokenU8;
                            if (!Enum.IsDefined(typeof(StackItemType), type))
                                throw new InvalidOperationException($"Invalid type for {instruction.OpCode}: {instruction.TokenU8}");
                            item = instruction.TokenU8 switch
                            {
                                (byte)StackItemType.Boolean => StackItem.False,
                                (byte)StackItemType.Integer => Integer.Zero,
                                (byte)StackItemType.ByteString => ByteString.Empty,
                                _ => StackItem.Null
                            };
                        }
                        else
                        {
                            item = StackItem.Null;
                        }
                        Push(new VMArray(ReferenceCounter, Enumerable.Repeat(item, n)));
                        break;
                    }
                case OpCode.NEWSTRUCT0:
                    {
                        Push(new Struct(ReferenceCounter));
                        break;
                    }
                case OpCode.NEWSTRUCT:
                    {
                        int n = (int)Pop().ReUse().GetInteger();
                        if (n < 0 || n > Limits.MaxStackSize)
                            throw new InvalidOperationException($"MaxStackSize exceed: {n}");
                        Struct result = new(ReferenceCounter);
                        for (var i = 0; i < n; i++)
                            result.Add(StackItem.Null);
                        Push(result);
                        break;
                    }
                case OpCode.NEWMAP:
                    {
                        Push(new Map(ReferenceCounter));
                        break;
                    }
                case OpCode.SIZE:
                    {
                        var x = Pop();
                        switch (x)
                        {
                            case CompoundType compound:
                                PushInteger(compound.Count);
                                break;
                            case PrimitiveType primitive:
                                PushInteger(primitive.Size);
                                break;
                            case Buffer buffer:
                                PushInteger(buffer.Size);
                                break;
                            default:
                                throw new InvalidOperationException($"Invalid type for {instruction.OpCode}: {x.Type}");
                        }
                        break;
                    }
                case OpCode.HASKEY:
                    {
                        PrimitiveType key = Pop<PrimitiveType>();
                        var x = Pop();
                        switch (x)
                        {
                            case VMArray array:
                                {
                                    int index = (int)key.GetInteger();
                                    if (index < 0)
                                        throw new InvalidOperationException($"The negative value {index} is invalid for OpCode.{instruction.OpCode}.");
                                    PushBoolean(index < array.Count);
                                    break;
                                }
                            case Map map:
                                {
                                    PushBoolean(map.ContainsKey(key));
                                    break;
                                }
                            case Buffer buffer:
                                {
                                    int index = (int)key.GetInteger();
                                    if (index < 0)
                                        throw new InvalidOperationException($"The negative value {index} is invalid for OpCode.{instruction.OpCode}.");
                                    PushBoolean(index < buffer.Size);
                                    break;
                                }
                            case ByteString array:
                                {
                                    int index = (int)key.GetInteger();
                                    if (index < 0)
                                        throw new InvalidOperationException($"The negative value {index} is invalid for OpCode.{instruction.OpCode}.");
                                    PushBoolean(index < array.Size);
                                    break;
                                }
                            default:
                                throw new InvalidOperationException($"Invalid type for {instruction.OpCode}: {x.Type}");
                        }
                        break;
                    }
                case OpCode.KEYS:
                    {
                        Map map = Pop<Map>();
                        Push(new VMArray(ReferenceCounter, map.Keys));
                        break;
                    }
                case OpCode.VALUES:
                    {
                        var x = Pop();
                        IEnumerable<StackItem> values = x switch
                        {
                            VMArray array => array,
                            Map map => map.Values,
                            _ => throw new InvalidOperationException($"Invalid type for {instruction.OpCode}: {x.Type}"),
                        };
                        VMArray newArray = new(ReferenceCounter);
                        foreach (StackItem item in values)
                            if (item is Struct s)
                                newArray.Add(s.Clone(Limits));
                            else
                                newArray.Add(item);
                        Push(newArray);
                        break;
                    }
                case OpCode.PICKITEM:
                    {
                        PrimitiveType key = Pop<PrimitiveType>();
                        var x = Pop();
                        switch (x)
                        {
                            case VMArray array:
                                {
                                    int index = (int)key.GetInteger();
                                    if (index < 0 || index >= array.Count)
                                        throw new CatchableException($"The value {index} is out of range.");
                                    Push(array[index]);
                                    break;
                                }
                            case Map map:
                                {
                                    if (!map.TryGetValue(key, out StackItem? value))
                                        throw new CatchableException($"Key not found in {nameof(Map)}");
                                    Push(value);
                                    break;
                                }
                            case PrimitiveType primitive:
                                {
                                    ReadOnlySpan<byte> byteArray = primitive.GetSpan();
                                    int index = (int)key.GetInteger();
                                    if (index < 0 || index >= byteArray.Length)
                                        throw new CatchableException($"The value {index} is out of range.");
                                    Push((BigInteger)byteArray[index]);
                                    break;
                                }
                            case Buffer buffer:
                                {
                                    int index = (int)key.GetInteger();
                                    if (index < 0 || index >= buffer.Size)
                                        throw new CatchableException($"The value {index} is out of range.");
                                    Push((BigInteger)buffer.InnerBuffer.Span[index]);
                                    break;
                                }
                            default:
                                throw new InvalidOperationException($"Invalid type for {instruction.OpCode}: {x.Type}");
                        }
                        break;
                    }
                case OpCode.APPEND:
                    {
                        StackItem newItem = Pop();
                        VMArray array = Pop<VMArray>();
                        if (newItem is Struct s) newItem = s.Clone(Limits);
                        array.Add(newItem);
                        break;
                    }
                case OpCode.SETITEM:
                    {
                        StackItem value = Pop();
                        if (value is Struct s) value = s.Clone(Limits);
                        PrimitiveType key = Pop<PrimitiveType>();
                        var x = Pop();
                        switch (x)
                        {
                            case VMArray array:
                                {
                                    int index = (int)key.GetInteger();
                                    if (index < 0 || index >= array.Count)
                                        throw new CatchableException($"The value {index} is out of range.");
                                    array[index] = value;
                                    break;
                                }
                            case Map map:
                                {
                                    map[key] = value;
                                    break;
                                }
                            case Buffer buffer:
                                {
                                    int index = (int)key.GetInteger();
                                    if (index < 0 || index >= buffer.Size)
                                        throw new CatchableException($"The value {index} is out of range.");
                                    if (value is not PrimitiveType p)
                                        throw new InvalidOperationException($"Value must be a primitive type in {instruction.OpCode}");
                                    int b = (int)p.GetInteger();
                                    if (b < sbyte.MinValue || b > byte.MaxValue)
                                        throw new InvalidOperationException($"Overflow in {instruction.OpCode}, {b} is not a byte type.");
                                    buffer.InnerBuffer.Span[index] = (byte)b;
                                    break;
                                }
                            default:
                                throw new InvalidOperationException($"Invalid type for {instruction.OpCode}: {x.Type}");
                        }
                        break;
                    }
                case OpCode.REVERSEITEMS:
                    {
                        var x = Pop();
                        switch (x)
                        {
                            case VMArray array:
                                array.Reverse();
                                break;
                            case Buffer buffer:
                                buffer.InnerBuffer.Span.Reverse();
                                break;
                            default:
                                throw new InvalidOperationException($"Invalid type for {instruction.OpCode}: {x.Type}");
                        }
                        break;
                    }
                case OpCode.REMOVE:
                    {
                        PrimitiveType key = Pop<PrimitiveType>();
                        var x = Pop();
                        switch (x)
                        {
                            case VMArray array:
                                int index = (int)key.GetInteger();
                                if (index < 0 || index >= array.Count)
                                    throw new InvalidOperationException($"The value {index} is out of range.");
                                array.RemoveAt(index);
                                break;
                            case Map map:
                                map.Remove(key);
                                break;
                            default:
                                throw new InvalidOperationException($"Invalid type for {instruction.OpCode}: {x.Type}");
                        }
                        break;
                    }
                case OpCode.CLEARITEMS:
                    {
                        CompoundType x = Pop<CompoundType>();
                        x.Clear();
                        break;
                    }
                case OpCode.POPITEM:
                    {
                        VMArray x = Pop<VMArray>();
                        int index = x.Count - 1;
                        Push(x[index]);
                        x.RemoveAt(index);
                        break;
                    }

                //Types
                case OpCode.ISNULL:
                    {
                        var x = Pop();
                        PushBoolean(x.IsNull);
                        break;
                    }
                case OpCode.ISTYPE:
                    {
                        var x = Pop();
                        StackItemType type = (StackItemType)instruction.TokenU8;
                        if (type == StackItemType.Any || !Enum.IsDefined(typeof(StackItemType), type))
                            throw new InvalidOperationException($"Invalid type: {type}");
                        PushBoolean(x.Type == type);
                        break;
                    }
                case OpCode.CONVERT:
                    {
                        var x = Pop();
                        Push(x.ConvertTo((StackItemType)instruction.TokenU8));
                        break;
                    }
                case OpCode.ABORTMSG:
                    {
                        var msg = Pop().ReUse().GetString();
                        throw new Exception($"{OpCode.ABORTMSG} is executed. Reason: {msg}");
                    }
                case OpCode.ASSERTMSG:
                    {
                        var msg = Pop().ReUse().GetString();
                        var x = Pop().ReUse().GetBoolean();
                        if (!x)
                            throw new Exception($"{OpCode.ASSERTMSG} is executed with false result. Reason: {msg}");
                        break;
                    }
                default: throw new InvalidOperationException($"Opcode {instruction.OpCode} is undefined.");
            }
        }


    }
}
