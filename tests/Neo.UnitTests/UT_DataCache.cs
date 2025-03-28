// Copyright (C) 2015-2025 The Neo Project.
//
// UT_DataCache.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Persistence;
using Neo.SmartContract;
using System;
using System.Linq;

namespace Neo.UnitTests
{
    [TestClass]
    public class UT_DataCache
    {
        [TestMethod]
        public void TestCachedFind_Between()
        {
            var snapshotCache = TestBlockchain.GetTestSnapshotCache();
            var storages = snapshotCache.CloneCache();
            var cache = new ClonedCache(storages);

            storages.Add(
                new StorageKey() { Key = new byte[] { 0x01, 0x01 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );
            storages.Add(
                new StorageKey() { Key = new byte[] { 0x00, 0x01 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );
            storages.Add(
                new StorageKey() { Key = new byte[] { 0x00, 0x03 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );
            cache.Add(
                new StorageKey() { Key = new byte[] { 0x01, 0x02 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );
            cache.Add(
                new StorageKey() { Key = new byte[] { 0x00, 0x02 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );

            CollectionAssert.AreEqual(
                cache.Find(new byte[5]).Select(u => u.Key.Key.Span[1]).ToArray(),
                new byte[] { 0x01, 0x02, 0x03 }
            );
        }

        [TestMethod]
        public void TestCachedFind_Last()
        {
            var snapshotCache = TestBlockchain.GetTestSnapshotCache();
            var storages = snapshotCache.CloneCache();
            var cache = new ClonedCache(storages);

            storages.Add(
                new StorageKey() { Key = new byte[] { 0x00, 0x01 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );
            storages.Add(
                new StorageKey() { Key = new byte[] { 0x01, 0x01 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );
            cache.Add(
                new StorageKey() { Key = new byte[] { 0x00, 0x02 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );
            cache.Add(
                new StorageKey() { Key = new byte[] { 0x01, 0x02 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );
            CollectionAssert.AreEqual(
                cache.Find(new byte[5]).Select(u => u.Key.Key.Span[1]).ToArray(),
                new byte[] { 0x01, 0x02 }
             );
        }

        [TestMethod]
        public void TestCachedFind_Empty()
        {
            var snapshotCache = TestBlockchain.GetTestSnapshotCache();
            var storages = snapshotCache.CloneCache();
            var cache = new ClonedCache(storages);

            cache.Add(
                new StorageKey() { Key = new byte[] { 0x00, 0x02 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );
            cache.Add(
                new StorageKey() { Key = new byte[] { 0x01, 0x02 }, Id = 0 },
                new StorageItem() { Value = ReadOnlyMemory<byte>.Empty }
            );

            CollectionAssert.AreEqual(
                cache.Find(new byte[5]).Select(u => u.Key.Key.Span[1]).ToArray(),
                new byte[] { 0x02 }
            );
        }
    }
}
