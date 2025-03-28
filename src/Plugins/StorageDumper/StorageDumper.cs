// Copyright (C) 2015-2025 The Neo Project.
//
// StorageDumper.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.ConsoleService;
using Neo.Extensions;
using Neo.IEventHandlers;
using Neo.Json;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract.Native;

namespace Neo.Plugins.StorageDumper
{
    public class StorageDumper : Plugin, ICommittingHandler, ICommittedHandler
    {
        private NeoSystem? _system;

        private StreamWriter? _writer;
        /// <summary>
        /// _currentBlock stores the last cached item
        /// </summary>
        private JObject? _currentBlock;
        private string? _lastCreateDirectory;
        protected override UnhandledExceptionPolicy ExceptionPolicy => Settings.Default?.ExceptionPolicy ?? UnhandledExceptionPolicy.Ignore;

        public override string Description => "Exports Neo-CLI status data";

        public override string ConfigFile => System.IO.Path.Combine(RootPath, "StorageDumper.json");

        public StorageDumper()
        {
            Blockchain.Committing += ((ICommittingHandler)this).Blockchain_Committing_Handler;
            Blockchain.Committed += ((ICommittedHandler)this).Blockchain_Committed_Handler;
        }

        public override void Dispose()
        {
            Blockchain.Committing -= ((ICommittingHandler)this).Blockchain_Committing_Handler;
            Blockchain.Committed -= ((ICommittedHandler)this).Blockchain_Committed_Handler;
        }

        protected override void Configure()
        {
            Settings.Load(GetConfiguration());
        }

        protected override void OnSystemLoaded(NeoSystem system)
        {
            _system = system;
        }

        /// <summary>
        /// Process "dump contract-storage" command
        /// </summary>
        [ConsoleCommand("dump contract-storage", Category = "Storage", Description = "You can specify the contract script hash or use null to get the corresponding information from the storage")]
        internal void OnDumpStorage(UInt160? contractHash = null)
        {
            if (_system == null) throw new InvalidOperationException("system doesn't exists");
            var path = $"dump_{_system.Settings.Network}.json";
            byte[]? prefix = null;
            if (contractHash is not null)
            {
                var contract = NativeContract.ContractManagement.GetContract(_system.StoreView, contractHash);
                if (contract is null) throw new InvalidOperationException("contract not found");
                prefix = BitConverter.GetBytes(contract.Id);
            }
            var states = _system.StoreView.Find(prefix);
            JArray array = new JArray(states.Where(p => !Settings.Default!.Exclude.Contains(p.Key.Id)).Select(p => new JObject
            {
                ["key"] = Convert.ToBase64String(p.Key.ToArray()),
                ["value"] = Convert.ToBase64String(p.Value.ToArray())
            }));
            File.WriteAllText(path, array.ToString());
            ConsoleHelper.Info("States",
                $"({array.Count})",
                " have been dumped into file ",
                $"{path}");
        }

        void ICommittingHandler.Blockchain_Committing_Handler(NeoSystem system, Block block, DataCache snapshot, IReadOnlyList<Blockchain.ApplicationExecuted> applicationExecutedList)
        {
            InitFileWriter(system.Settings.Network, snapshot);
            OnPersistStorage(system.Settings.Network, snapshot);
        }

        private void OnPersistStorage(uint network, DataCache snapshot)
        {
            var blockIndex = NativeContract.Ledger.CurrentIndex(snapshot);
            if (blockIndex >= Settings.Default!.HeightToBegin)
            {
                var stateChangeArray = new JArray();

                foreach (var trackable in snapshot.GetChangeSet())
                {
                    if (Settings.Default.Exclude.Contains(trackable.Key.Id))
                        continue;
                    var state = new JObject();
                    switch (trackable.Value.State)
                    {
                        case TrackState.Added:
                            state["id"] = trackable.Key.Id;
                            state["state"] = "Added";
                            state["key"] = Convert.ToBase64String(trackable.Key.ToArray());
                            state["value"] = Convert.ToBase64String(trackable.Value.Item.ToArray());
                            break;
                        case TrackState.Changed:
                            state["id"] = trackable.Key.Id;
                            state["state"] = "Changed";
                            state["key"] = Convert.ToBase64String(trackable.Key.ToArray());
                            state["value"] = Convert.ToBase64String(trackable.Value.Item.ToArray());
                            break;
                        case TrackState.Deleted:
                            state["id"] = trackable.Key.Id;
                            state["state"] = "Deleted";
                            state["key"] = Convert.ToBase64String(trackable.Key.ToArray());
                            break;
                    }
                    stateChangeArray.Add(state);
                }

                var bs_item = new JObject();
                bs_item["block"] = blockIndex;
                bs_item["size"] = stateChangeArray.Count;
                bs_item["storage"] = stateChangeArray;
                _currentBlock = bs_item;
            }
        }


        void ICommittedHandler.Blockchain_Committed_Handler(NeoSystem system, Block block)
        {
            OnCommitStorage(system.Settings.Network);
        }

        void OnCommitStorage(uint network)
        {
            if (_currentBlock != null && _writer != null)
            {
                _writer.WriteLine(_currentBlock.ToString());
                _writer.Flush();
            }
        }

        private void InitFileWriter(uint network, IReadOnlyStore snapshot)
        {
            uint blockIndex = NativeContract.Ledger.CurrentIndex(snapshot);
            if (_writer == null
                || blockIndex % Settings.Default!.BlockCacheSize == 0)
            {
                string path = GetOrCreateDirectory(network, blockIndex);
                var filepart = (blockIndex / Settings.Default!.BlockCacheSize) * Settings.Default.BlockCacheSize;
                path = $"{path}/dump-block-{filepart}.dump";
                if (_writer != null)
                {
                    _writer.Dispose();
                }
                _writer = new StreamWriter(new FileStream(path, FileMode.Append));
            }
        }

        private string GetOrCreateDirectory(uint network, uint blockIndex)
        {
            string dirPathWithBlock = GetDirectoryPath(network, blockIndex);
            if (_lastCreateDirectory != dirPathWithBlock)
            {
                Directory.CreateDirectory(dirPathWithBlock);
                _lastCreateDirectory = dirPathWithBlock;
            }
            return dirPathWithBlock;
        }

        private string GetDirectoryPath(uint network, uint blockIndex)
        {
            uint folder = (blockIndex / Settings.Default!.StoragePerFolder) * Settings.Default.StoragePerFolder;
            return $"./StorageDumper_{network}/BlockStorage_{folder}";
        }

    }
}
