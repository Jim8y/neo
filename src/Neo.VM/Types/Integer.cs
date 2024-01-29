// Copyright (C) 2015-2024 The Neo Project.
//
// Integer.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.IO.ObjectPool;
using Neo.VM.Types.ObjectPool;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Neo.VM.Types
{
    /// <summary>
    /// Represents an integer value in the VM.
    /// </summary>
    [DebuggerDisplay("Type={GetType().Name}, Value={value}")]
    public class Integer : PrimitiveType, IPoolable<BigInteger>
    {
        /// <summary>
        /// The maximum size of an integer in bytes.
        /// </summary>
        public const int MaxSize = 32;

        /// <summary>
        /// Represents the number 0.
        /// </summary>
        public static readonly Integer Zero = 0;
        private BigInteger value;

        public override ReadOnlyMemory<byte> Memory
        {
            get => value.IsZero ? ReadOnlyMemory<byte>.Empty : value.ToByteArray();
            set => throw new NotImplementedException();
        }

        public override int Size { get; set; }
        public override StackItemType Type => StackItemType.Integer;

        /// <summary>
        /// Create an integer with the specified value.
        /// </summary>
        /// <param name="value">The value of the integer.</param>
        // public Integer(BigInteger value)
        // {
        //     if (value.IsZero)
        //     {
        //         Size = 0;
        //     }
        //     else
        //     {
        //         Size = value.GetByteCount();
        //         if (Size > MaxSize) throw new ArgumentException($"MaxSize exceed: {Size}");
        //     }
        //     this.value = value;
        // }

        public Integer() { }

        public override bool Equals(StackItem? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is Integer i) return value == i.value;
            return false;
        }

        public override bool GetBoolean()
        {
            return !value.IsZero;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(value);
        }

        public override BigInteger GetInteger()
        {
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Integer(sbyte value)
        {
            return ObjectFactory.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Integer(byte value)
        {
            return ObjectFactory.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Integer(short value)
        {
            return ObjectFactory.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Integer(ushort value)
        {
            return ObjectFactory.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Integer(int value)
        {
            return ObjectFactory.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Integer(uint value)
        {
            return ObjectFactory.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Integer(long value)
        {
            return ObjectFactory.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Integer(ulong value)
        {
            return ObjectFactory.Get(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Integer(BigInteger value)
        {
            return ObjectFactory.Get(value);
        }

        public void SetValue(BigInteger value)
        {
            if (value.IsZero)
            {
                Size = 0;
            }
            else
            {
                Size = value.GetByteCount();
                if (Size > MaxSize) throw new ArgumentException($"MaxSize exceed: {Size}");
            }
            this.value = value;
        }
    }
}
