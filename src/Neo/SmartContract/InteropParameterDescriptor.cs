// Copyright (C) 2015-2025 The Neo Project.
//
// InteropParameterDescriptor.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.Cryptography.ECC;
using Neo.VM.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Array = Neo.VM.Types.Array;
using Pointer = Neo.VM.Types.Pointer;

namespace Neo.SmartContract
{
    /// <summary>
    /// Represents a descriptor of an interoperable service parameter.
    /// </summary>
    public class InteropParameterDescriptor
    {
        private readonly ValidatorAttribute[] _validators;

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The converter to convert the parameter from <see cref="StackItem"/> to <see cref="object"/>.
        /// </summary>
        public Func<StackItem, object> Converter { get; }

        /// <summary>
        /// Indicates whether the parameter is an enumeration.
        /// </summary>
        public bool IsEnum => Type.IsEnum;

        /// <summary>
        /// Indicates whether the parameter is an array.
        /// </summary>
        public bool IsArray => Type.IsArray && Type.GetElementType() != typeof(byte);

        /// <summary>
        /// Indicates whether the parameter is an <see cref="InteropInterface"/>.
        /// </summary>
        public bool IsInterface { get; }

        private static readonly Dictionary<Type, Func<StackItem, object>> converters = new()
        {
            [typeof(StackItem)] = p => p,
            [typeof(Pointer)] = p => p,
            [typeof(Array)] = p => p,
            [typeof(InteropInterface)] = p => p,
            [typeof(bool)] = p => p.GetBoolean(),
            [typeof(sbyte)] = p => (sbyte)p.GetInteger(),
            [typeof(byte)] = p => (byte)p.GetInteger(),
            [typeof(short)] = p => (short)p.GetInteger(),
            [typeof(ushort)] = p => (ushort)p.GetInteger(),
            [typeof(int)] = p => (int)p.GetInteger(),
            [typeof(uint)] = p => (uint)p.GetInteger(),
            [typeof(long)] = p => (long)p.GetInteger(),
            [typeof(ulong)] = p => (ulong)p.GetInteger(),
            [typeof(BigInteger)] = p => p.GetInteger(),
            [typeof(byte[])] = p => p.IsNull ? null : p.GetSpan().ToArray(),
            [typeof(string)] = p => p.IsNull ? null : p.GetString(),
            [typeof(UInt160)] = p => p.IsNull ? null : new UInt160(p.GetSpan()),
            [typeof(UInt256)] = p => p.IsNull ? null : new UInt256(p.GetSpan()),
            [typeof(ECPoint)] = p => p.IsNull ? null : ECPoint.DecodePoint(p.GetSpan(), ECCurve.Secp256r1),
        };

        internal InteropParameterDescriptor(ParameterInfo parameterInfo)
            : this(parameterInfo.ParameterType, parameterInfo.GetCustomAttributes<ValidatorAttribute>(true).ToArray())
        {
            Name = parameterInfo.Name;
        }

        internal InteropParameterDescriptor(Type type, params ValidatorAttribute[] validators)
        {
            Type = type;
            _validators = validators;
            if (IsEnum)
            {
                Converter = converters[type.GetEnumUnderlyingType()];
            }
            else if (IsArray)
            {
                Converter = converters[type.GetElementType()];
            }
            else
            {
                IsInterface = !converters.TryGetValue(type, out var converter);
                if (IsInterface)
                    Converter = converters[typeof(InteropInterface)];
                else
                    Converter = converter;
            }
        }

        public void Validate(StackItem item)
        {
            foreach (var validator in _validators)
                validator.Validate(item);
        }
    }
}
