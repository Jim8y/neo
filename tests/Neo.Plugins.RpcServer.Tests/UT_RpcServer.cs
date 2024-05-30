// Copyright (C) 2015-2024 The Neo Project.
//
// UT_RpcServer.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using RpcServerClass = Neo.Plugins.RpcServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Neo.Ledger;
using Neo.Persistence;

namespace Neo.Plugins.RpcServer.Tests
{
    [TestClass]
    public partial class UT_RpcServer
    {
        private Mock<NeoSystem> mockSystem;
        private Mock<DataCache> mockStoreView;
        private Mock<MemoryPool> mockMemPool;
        private RpcServerClass rpcServer;

        [TestInitialize]
        public void Initialize()
        {
            // Set up the mock system
            mockSystem = new Mock<NeoSystem>(null, (string)null);

            // Set up the mock store view
            mockStoreView = new Mock<DataCache>();
            mockSystem.SetupGet(s => s.StoreView).Returns(mockStoreView.Object);

            // Set up the mock memory pool
            mockMemPool = new Mock<MemoryPool>(mockSystem.Object);
            mockSystem.SetupGet(s => s.MemPool).Returns(mockMemPool.Object);

            // Initialize the RpcServer with the mock system
            rpcServer = new RpcServer(mockSystem.Object, new RpcServerSettings());
        }
    }
}
