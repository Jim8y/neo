// Copyright (C) 2015-2025 The Neo Project.
//
// UT_MethodToken.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Extensions;
using Neo.SmartContract;
using System;

namespace Neo.UnitTests.SmartContract
{
    [TestClass]
    public class UT_MethodToken
    {
        [TestMethod]
        public void TestSerialize()
        {
            var result = new MethodToken()
            {
                CallFlags = CallFlags.AllowCall,
                Hash = UInt160.Parse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01"),
                Method = "myMethod",
                ParametersCount = 123,
                HasReturnValue = true
            };

            var copy = result.ToArray().AsSerializable<MethodToken>();

            Assert.AreEqual(CallFlags.AllowCall, copy.CallFlags);
            Assert.AreEqual("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01", copy.Hash.ToString());
            Assert.AreEqual("myMethod", copy.Method);
            Assert.AreEqual(123, copy.ParametersCount);
            Assert.IsTrue(copy.HasReturnValue);
        }

        [TestMethod]
        public void TestSerializeErrors()
        {
            var result = new MethodToken()
            {
                CallFlags = (CallFlags)byte.MaxValue,
                Hash = UInt160.Parse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01"),
                Method = "myLongMethod",
                ParametersCount = 123,
                HasReturnValue = true
            };

            Assert.ThrowsExactly<FormatException>(() => _ = result.ToArray().AsSerializable<MethodToken>());

            result.CallFlags = CallFlags.All;
            result.Method += "-123123123123123123123123";
            Assert.ThrowsExactly<FormatException>(() => _ = result.ToArray().AsSerializable<MethodToken>());
        }
    }
}
