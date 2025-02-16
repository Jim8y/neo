// Copyright (C) 2015-2025 The Neo Project.
//
// StatelessBlock.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.ConsoleService;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Persistence.Providers;
using Neo.Plugins.StatelessBlock.Store;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Extensions;
using Neo.IEventHandlers;
using static System.IO.Path;

namespace Neo.Plugins.StatelessBlock
{
    /// <summary>
    /// The StatelessBlock plugin enables stateless transaction execution by recording and managing the minimal
    /// storage data sets required for transaction execution.
    ///
    /// Key Features:
    /// 1. Efficient Storage: Uses a two-level storage system:
    ///    - Transaction level: Maps storage keys to value hashes
    ///    - Global level: Stores unique values indexed by their hashes
    ///
    /// 2. Data Deduplication:
    ///    - Identical values across different transactions are stored only once
    ///    - Different transactions can reference the same stored value using its hash
    ///
    /// 3. Transaction Independence:
    ///    - Each transaction maintains its own read set mapping
    ///    - Handles cases where different transactions read different values for the same key
    ///
    /// This design allows transactions to be re-executed with only their necessary state,
    /// without requiring access to the full blockchain state.
    /// </summary>
    public class StatelessBlock : Plugin,ICommittingHandler
    {
        public static StatelessBlock Instance { get; private set; }

        private IStore _store;
        private NeoSystem _neoSystem;
        private ReadSetStore _readSetStore;
        private bool _initialized;

        public override string Name => "StatelessBlock";
        public override string Description => "Tracks storage reads for stateless block execution";
        protected override UnhandledExceptionPolicy ExceptionPolicy => UnhandledExceptionPolicy.StopPlugin;

        public StatelessBlock()
        {
            Instance = this;
            RegisterEventHandlers();
        }

        public override string ConfigFile => Combine(RootPath, "StatelessBlock.json");

        private void RegisterEventHandlers()
        {
            try
            {
                Blockchain.Committing += Blockchain_Committing_Handler;
            }
            catch (Exception ex)
            {
                Log($"Failed to register event handlers: {ex.Message}");
                throw;
            }
        }

        private void UnregisterEventHandlers()
        {
            Blockchain.Committing -= Blockchain_Committing_Handler;
        }

        protected override void Configure()
        {
            try
            {
                Settings.Load(GetConfiguration());
                Log($"StatelessBlock plugin configured with MaxCachedReadSets: {Settings.Default.MaxCachedReadSets}");
            }
            catch (Exception ex)
            {
                Log($"Error loading configuration: {ex.Message}");
                throw;
            }
        }

        protected override void OnSystemLoaded(NeoSystem system)
        {
            ArgumentNullException.ThrowIfNull(system);

            try
            {
                _neoSystem = system;
                InitializeStore();
                _initialized = true;
                Log($"StatelessBlock plugin initialized successfully");
            }
            catch (Exception ex)
            {
                Log($"Error during system loading: {ex.Message}");
                throw;
            }
        }

        private void InitializeStore()
        {
            try
            {
                if (string.IsNullOrEmpty(Settings.Default.Provider))
                {
                    // Use the same provider as the main chain if not specified
                    _store = _neoSystem.LoadStore(GetStoragePath());
                }
                else
                {
                    // Use the specified provider
                    var provider = Settings.Default.Provider;
                    _store = _neoSystem.LoadStore(GetStoragePath(), provider);
                }

                if (_store is null)
                {
                    throw new InvalidOperationException("Failed to initialize storage provider");
                }

                _readSetStore = new ReadSetStore(_store);
                Log($"Store initialized at {GetStoragePath()}");
            }
            catch (Exception ex)
            {
                Log($"Failed to initialize store: {ex.Message}");
                throw;
            }
        }

        private string GetStoragePath()
        {
            return GetFullPath(string.Format(Settings.Default.Path, _neoSystem.Settings.Network.ToString("X8")));
        }

        public override void Dispose()
        {
            try
            {
                UnregisterEventHandlers();
                _readSetStore?.Dispose();
                _store?.Dispose();
                _neoSystem = null;
                Instance = null;
                _initialized = false;
            }
            catch (Exception ex)
            {
                Log($"Error during plugin disposal: {ex.Message}");
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Processes block execution and stores the minimal required read set.
        /// Only stores the first read value for each storage key, ignoring subsequent modifications.
        /// </summary>
        public void Blockchain_Committing_Handler(NeoSystem system, Block block, DataCache snapshot,
            IReadOnlyList<Blockchain.ApplicationExecuted> applicationExecutedList)
        {
            if (!_initialized)
            {
                Log("Plugin not yet initialized. Skipping OnCommitting.");
                return;
            }

            // Ignore the block if there is no transaction
            if (applicationExecutedList.Count == 0) return;

            try
            {
                // Get the read set from the snapshot and convert to byte arrays
                var originalReadSet = snapshot.GetReadSet();
                var comparer = new ByteArrayComparer(1);
                var readSet = new Dictionary<byte[], byte[]>(comparer);

                foreach (var (key, item) in originalReadSet)
                {
                    readSet[key.ToArray()] = item.Value.ToArray();
                }

                // Store the block's read set
                _readSetStore.Put(block.Hash, readSet);
                _readSetStore.Commit();

                Log($"Stored minimal read set for block {block.Hash}, containing {readSet.Count} unique storage entries");
            }
            catch (Exception ex)
            {
                Log($"Error in Blockchain_Committing_Handler: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the minimal read set required for a block's execution
        /// </summary>
        public bool TryGetBlockReadSet(UInt256 blockHash, out Dictionary<byte[], byte[]> readSet)
        {
            return _readSetStore.TryGet(blockHash, out readSet);
        }

        /// <summary>
        /// CLI command to execute a block using its stored minimal read set.
        /// This demonstrates stateless execution by:
        /// 1. Retrieving the block's minimal read set
        /// 2. Creating a memory store with just the required data
        /// 3. Executing the block using this minimal state
        /// </summary>
        [ConsoleCommand("stateless-block", Category = "StatelessBlock Commands", Description = "Execute a block using its minimal read set")]
        private void ExecuteBlockCLI(string blockHashStr)
        {
            try
            {
                if (!UInt256.TryParse(blockHashStr, out var blockHash))
                {
                    Console.WriteLine("Invalid block hash format");
                    return;
                }

                if (!TryGetBlockReadSet(blockHash, out var readSet))
                {
                    Console.WriteLine($"No read set found for block {blockHash}");
                    return;
                }

                var block = _neoSystem.Blockchain.GetBlock(blockHash);
                if (block == null)
                {
                    Console.WriteLine($"Block {blockHash} not found");
                    return;
                }

                // Create a memory store with just the required read set
                using var store = new MemoryStore();
                using var snapshot = store.GetSnapshot();

                // Populate the store with the minimal read set
                foreach (var (key, value) in readSet)
                {
                    snapshot.Put(key, value);
                }
                snapshot.Commit();

                // Execute the block's transactions
                foreach (var tx in block.Transactions)
                {
                    using var engine = ApplicationEngine.Create(TriggerType.Application, tx, snapshot.CreateSnapshot(), tx.SystemFee);

                    try
                    {
                        engine.Execute();
                        Console.WriteLine($"Transaction {tx.Hash} executed successfully");
                        if (engine.State == VMState.FAULT)
                        {
                            Console.WriteLine($"Execution failed: {engine.FaultException?.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing transaction {tx.Hash}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the read set for a specific transaction
        /// </summary>
        public bool TryGetReadSet(UInt256 txHash, out Dictionary<byte[], byte[]> readSet)
        {
            if (!_initialized)
            {
                Log("Plugin not yet initialized. Cannot get read set.");
                readSet = null;
                return false;
            }

            try
            {
                return _readSetStore.TryGet(txHash, out readSet);
            }
            catch (Exception ex)
            {
                Log($"Error retrieving read set for tx {txHash}: {ex.Message}");
                readSet = null;
                return false;
            }
        }

        /// <summary>
        /// CLI command to execute a transaction using its stored read set.
        /// This demonstrates stateless execution by:
        /// 1. Retrieving the transaction's read set from storage
        /// 2. Creating a memory store with just the required data
        /// 3. Executing the transaction using this minimal state
        /// </summary>
        /// <param name="txHashStr">The transaction hash in string format</param>
        public void ExecuteTransactionCLI(string txHashStr)
        {
            // Parse the transaction hash
            if (!UInt256.TryParse(txHashStr, out var txHash))
            {
                Console.WriteLine($"Invalid transaction hash: {txHashStr}");
                return;
            }

            // Try to retrieve the read set for the transaction
            if (TryGetReadSet(txHash, out var readSet))
            {
                Console.WriteLine($"Executing transaction with hash: {txHashStr}");

                // Simulated execution: print each key/value in the read set
                foreach (var kv in readSet)
                {
                    Console.WriteLine($"Key: {Convert.ToBase64String(kv.Key)}, Value: {Convert.ToBase64String(kv.Value)}");
                }

                // Here you could add additional logic to re-execute the transaction using the read set
            }
            else
            {
                Console.WriteLine($"Read set for transaction {txHashStr} not found.");
            }
        }

        [ConsoleCommand("execute transaction", Category = "Stateless Block Commands", Description = "Execute a transaction using its read set")]
        private void OnExecuteTransactionWithReadSet(string txHashStr)
        {
            if (!UInt256.TryParse(txHashStr, out var txHash))
            {
                ConsoleHelper.Error("Invalid transaction hash format");
                return;
            }

            // Get the read set from the store
            if (!TryGetReadSet(txHash, out var readSet))
            {
                ConsoleHelper.Error($"Read set not found for transaction {txHash}");
                return;
            }

            // Get the transaction from the blockchain
            var tx = NativeContract.Ledger.GetTransactionState(_neoSystem.StoreView, txHash)?.Transaction;
            if (tx == null)
            {
                ConsoleHelper.Error($"Transaction {txHash} not found in blockchain");
                return;
            }

            try
            {
                // Create a memory store with the read set data
                var memoryStore = new MemoryStore();
                foreach (var (key, value) in readSet)
                {
                    memoryStore.Put(key, value);
                }

                // Create a snapshot from the memory store
                using var snapshot =new StoreCache(memoryStore.GetSnapshot());

                // Create application engine for execution
                using var engine = ApplicationEngine.Create(TriggerType.Application, tx, snapshot, tx.SystemFee);

                ConsoleHelper.Info("Executing transaction with read set...");
                ConsoleHelper.Info($"Transaction Hash: {txHash}");
                ConsoleHelper.Info($"Read Set Size: {readSet.Count} items");

                // Execute the transaction
                engine.Execute();

                // Display execution results
                ConsoleHelper.Info($"Execution completed with state: {engine.State}");
                ConsoleHelper.Info($"Gas consumed: {engine.GasConsumed}");

                if (engine.State == VMState.FAULT)
                {
                    ConsoleHelper.Error($"Execution failed: {engine.FaultException?.Message}");
                }
                else
                {
                    // Display any results from the stack
                    if (engine.ResultStack.Count > 0)
                    {
                        ConsoleHelper.Info("Result stack:");
                        foreach (var item in engine.ResultStack)
                        {
                            ConsoleHelper.Info($"  {item.ToJson()}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Error executing transaction: {ex.Message}");
            }
        }

        public void OnAwared(NeoSystem neoSystem)
        {
            _neoSystem = neoSystem;
        }
    }
}
