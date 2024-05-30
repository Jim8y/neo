// Copyright (C) 2015-2024 The Neo Project.
//
// UT_RpcServer.Blockchain.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using RpcServerNamespace = Neo.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Neo.Json;
using System;

namespace Neo.Plugins.RpcServer.Tests;

public partial class UT_RpcServer
{
    [TestMethod]
    public void TestGetBestBlockHash()
    {
        // Mocking the response of CurrentHash
        mockStoreView.Setup(m => m.Get(It.IsAny<StorageKey>())).Returns(new StorageItem { Value = UInt256.Parse("0xabcdef1234567890").ToArray() });

        var result = rpcServer.GetBestBlockHash(new JArray());
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JString));
        Assert.AreEqual("0xabcdef1234567890", result.AsString());
    }

    [TestMethod]
    public void TestGetBlock()
    {
        // Mocking the response of GetBlock
        var mockBlock = new Mock<Block>();
        mockBlock.Setup(b => b.ToArray()).Returns(new byte[] { });
        mockStoreView.Setup(m => m.TryGet(It.IsAny<UInt256>(), out It.Ref<Block>.IsAny))
            .Callback(new TryGetDelegate<UInt256, Block>((UInt256 _, out Block block) => block = mockBlock.Object))
            .Returns(true);

        // Valid Block Hash
        var validBlockHash = "0xabcdef1234567890";
        var result = rpcServer.GetBlock(new JArray(validBlockHash, true));
        Assert.IsNotNull(result);

        // Invalid Block Hash
        var invalidBlockHash = "invalid_hash";
        Assert.ThrowsException<RpcException>(() => rpcServer.GetBlock(new JArray(invalidBlockHash, true)));

        // Valid Block Index
        var validBlockIndex = 0;
        result = rpcServer.GetBlock(new JArray(validBlockIndex, true));
        Assert.IsNotNull(result);

        // Invalid Block Index
        var invalidBlockIndex = -1;
        Assert.ThrowsException<RpcException>(() => rpcServer.GetBlock(new JArray(invalidBlockIndex, true)));
    }


    [TestMethod]
    public void TestGetBlockHeaderCount()
    {
        var mockSystem = new Mock<ISystem>();
        var rpcServer = new RpcServer(mockSystem.Object);
        var result = rpcServer.GetBlockHeaderCount(new JArray());
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(int));
    }

    [TestMethod]
    public void TestGetBlockCount()
    {
        mockStoreView.Setup(m => m.Get(It.IsAny<StorageKey>())).Returns(new StorageItem { Value = new byte[] { } });

        var result = rpcServer.GetBlockCount(new JArray());
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JNumber));
        Assert.AreEqual(1, result.AsNumber());
    }

    [TestMethod]
    public void TestGetBlockHash()
    {
        // Mocking the response of GetBlockHash
        mockStoreView.Setup(m => m.Get(It.IsAny<StorageKey>())).Returns(new StorageItem { Value = UInt256.Parse("0xabcdef1234567890").ToArray() });

        // Valid Height
        var validHeight = 1000;
        var result = rpcServer.GetBlockHash(new JArray(validHeight));
        Assert.IsNotNull(result);
        Assert.AreEqual("0xabcdef1234567890", result.AsString());

        // Invalid Height
        var invalidHeight = -1;
        Assert.ThrowsException<RpcException>(() => rpcServer.GetBlockHash(new JArray(invalidHeight)));
    }


    [TestMethod]
    public void TestGetBlockHeader()
    {
        var mockSystem = new Mock<ISystem>();
        var rpcServer = new RpcServer(mockSystem.Object);
        // Valid Block Hash
        var validBlockHash = "0x1234567890abcdef";
        var result = rpcServer.GetBlockHeader(new JArray(validBlockHash, true));
        Assert.IsNotNull(result);

        // Invalid Block Hash
        var invalidBlockHash = "invalid_hash";
        Assert.ThrowsException<RpcException>(() => rpcServer.GetBlockHeader(new JArray(invalidBlockHash, true)));

        // Valid Block Index
        var validBlockIndex = 0;
        result = rpcServer.GetBlockHeader(new JArray(validBlockIndex, true));
        Assert.IsNotNull(result);

        // Invalid Block Index
        var invalidBlockIndex = -1;
        Assert.ThrowsException<RpcException>(() => rpcServer.GetBlockHeader(new JArray(invalidBlockIndex, true)));
    }

    [TestMethod]
    public void TestGetContractState()
    {
        // Mocking the response of GetContract
        var mockContractState = new Mock<ContractState>();
        mockContractState.Setup(cs => cs.ToJson()).Returns(new JObject());
        mockStoreView.Setup(m => m.TryGet(It.IsAny<UInt160>(), out It.Ref<ContractState>.IsAny))
            .Callback(new TryGetDelegate<UInt160, ContractState>((UInt160 _, out ContractState contractState) => contractState = mockContractState.Object))
            .Returns(true);

        // Valid Contract ID
        var validContractId = 1;
        var result = rpcServer.GetContractState(new JArray(validContractId));
        Assert.IsNotNull(result);

        // Invalid Contract ID
        var invalidContractId = -1;
        Assert.ThrowsException<RpcException>(() => rpcServer.GetContractState(new JArray(invalidContractId)));

        // Valid Script Hash
        var validScriptHash = "0xabcdef1234567890";
        result = rpcServer.GetContractState(new JArray(validScriptHash));
        Assert.IsNotNull(result);

        // Invalid Script Hash
        var invalidScriptHash = "invalid_hash";
        Assert.ThrowsException<RpcException>(() => rpcServer.GetContractState(new JArray(invalidScriptHash)));
    }

    [TestMethod]
    public void TestGetRawMemPool()
    {
        // Mocking the response of GetVerifiedTransactions and GetUnverifiedTransactions
        mockMemPool.Setup(m => m.GetVerifiedTransactions()).Returns(new List<Transaction> { new Transaction() });
        mockMemPool.Setup(m => m.GetVerifiedAndUnverifiedTransactions(out It.Ref<IEnumerable<Transaction>>.IsAny, out It.Ref<IEnumerable<Transaction>>.IsAny))
            .Callback(new GetVerifiedAndUnverifiedTransactionsDelegate((out IEnumerable<Transaction> verifiedTransactions, out IEnumerable<Transaction> unverifiedTransactions) =>
            {
                verifiedTransactions = new List<Transaction> { new Transaction() };
                unverifiedTransactions = new List<Transaction> { new Transaction() };
            }));

        // Without Unverified Transactions
        var result = rpcServer.GetRawMemPool(new JArray(false));
        Assert.IsNotNull(result);

        // With Unverified Transactions
        result = rpcServer.GetRawMemPool(new JArray(true));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void TestGetRawTransaction()
    {
        // Mocking the response of GetTransactionState
        var mockTransactionState = new Mock<TransactionState>();
        mockTransactionState.Setup(m => m.Transaction).Returns(new Transaction());
        mockStoreView.Setup(m => m.TryGet(It.IsAny<UInt256>(), out It.Ref<TransactionState>.IsAny))
            .Callback(new TryGetDelegate<UInt256, TransactionState>((UInt256 _, out TransactionState transactionState) => transactionState = mockTransactionState.Object))
            .Returns(true);

        // Valid Transaction Hash
        var validTransactionHash = "0xabcdef1234567890";
        var result = rpcServer.GetRawTransaction(new JArray(validTransactionHash, true));
        Assert.IsNotNull(result);

        // Invalid Transaction Hash
        var invalidTransactionHash = "invalid_hash";
        Assert.ThrowsException<RpcException>(() => rpcServer.GetRawTransaction(new JArray(invalidTransactionHash, true)));
    }


    [TestMethod]
    public void TestGetStorage()
    {
        var mockSystem = new Mock<ISystem>();
        var rpcServer = new RpcServer(mockSystem.Object);
        // Valid Storage Key
        var validContractId = 1;
        var validKey = Convert.ToBase64String(new byte[] { 0x01, 0x02, 0x03 });
        var result = rpcServer.GetStorage(new JArray(validContractId, validKey));
        Assert.IsNotNull(result);

        // Invalid Storage Key
        var invalidContractId = -1;
        var invalidKey = "invalid_key";
        Assert.ThrowsException<RpcException>(() => rpcServer.GetStorage(new JArray(invalidContractId, invalidKey)));
    }

    [TestMethod]
    public void TestFindStorage()
    {
        var mockSystem = new Mock<ISystem>();
        var rpcServer = new RpcServer(mockSystem.Object);
        // Valid Storage Prefix
        var validContractId = 1;
        var validPrefix = Convert.ToBase64String(new byte[] { 0x01 });
        var result = rpcServer.FindStorage(new JArray(validContractId, validPrefix, 0));
        Assert.IsNotNull(result);

        // Invalid Storage Prefix
        var invalidContractId = -1;
        var invalidPrefix = "invalid_prefix";
        Assert.ThrowsException<RpcException>(() => rpcServer.FindStorage(new JArray(invalidContractId, invalidPrefix, 0)));
    }

    [TestMethod]
    public void TestGetTransactionHeight()
    {
        var mockSystem = new Mock<ISystem>();
        var rpcServer = new RpcServer(mockSystem.Object);
        // Valid Transaction Hash
        var validTransactionHash = "0xabcdef1234567890";
        var result = rpcServer.GetTransactionHeight(new JArray(validTransactionHash));
        Assert.IsNotNull(result);

        // Invalid Transaction Hash
        var invalidTransactionHash = "invalid_hash";
        Assert.ThrowsException<RpcException>(() => rpcServer.GetTransactionHeight(new JArray(invalidTransactionHash)));
    }

    [TestMethod]
    public void TestGetNextBlockValidators()
    {
        var mockSystem = new Mock<ISystem>();
        var rpcServer = new RpcServer(mockSystem.Object);
        var result = rpcServer.GetNextBlockValidators(new JArray());
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void TestGetCandidates()
    {
        var mockSystem = new Mock<ISystem>();
        var rpcServer = new RpcServer(mockSystem.Object);
        var result = rpcServer.GetCandidates(new JArray());
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void TestGetCommittee()
    {
        var mockSystem = new Mock<ISystem>();
        var rpcServer = new RpcServer(mockSystem.Object);
        var result = rpcServer.GetCommittee(new JArray());
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void TestGetNativeContracts()
    {
        var mockSystem = new Mock<ISystem>();
        var rpcServer = new RpcServer(mockSystem.Object);
        var result = rpcServer.GetNativeContracts(new JArray());
        Assert.IsNotNull(result);
    }


}
