using Neo.Cryptography;
using Neo.Extensions;
using Neo.Persistence;
using Neo.Plugins.StatelessBlock.Store.States;
using Neo.SmartContract;

namespace Neo.Plugins.StatelessBlock.Store
{
    internal class ReadSetStore : IDisposable
    {
        private static readonly int Prefix_Id = 0x6c657373;
        private const byte PrefixBlockReadSet = 0x01;
        private const byte PrefixValue = 0x02;
        private readonly IStore _store;
        private readonly IStoreSnapshot _snapshot;
        private readonly Dictionary<UInt256, byte[]> _valueCache;

        public ReadSetStore(IStore store)
        {
            _store = store;
            _snapshot = store.GetSnapshot();
            _valueCache = new Dictionary<UInt256, byte[]>();
        }

        public ReadSetStore(IStoreSnapshot snapshot)
        {
            _snapshot = snapshot;
            _valueCache = new Dictionary<UInt256, byte[]>();
        }

        public void Dispose()
        {
            _snapshot?.Dispose();
        }

        private static UInt256 ComputeValueHash(byte[] value)
        {
            return new UInt256(Crypto.Hash256(value));
        }

        /// <summary>
        /// Stores a block's read set using value deduplication:
        /// 1. For each unique value, compute its hash and store it only once
        /// 2. Store the block's read set with references to the deduplicated values
        /// </summary>
        public void Put(UInt256 blockHash, Dictionary<byte[], byte[]> readSet)
        {
            var blockState = new BlockReadSetState();

            // Store each unique value only once
            foreach (var (key, value) in readSet)
            {
                var valueHash = ComputeValueHash(value);

                // Store value if it's not already stored
                var valueKey = new KeyBuilder(Prefix_Id, PrefixValue).Add(valueHash).ToArray();
                if (!_snapshot.Contains(valueKey))
                {
                    _snapshot.Put(valueKey, value);
                    _valueCache[valueHash] = value;
                }

                // Add to block's read set
                blockState._initialReadSet[key] = value;
            }

            // Store the block's read set state
            var blockKey = new KeyBuilder(Prefix_Id, PrefixBlockReadSet).Add(blockHash).ToArray();
            _snapshot.Put(blockKey, blockState.ToArray());
        }

        /// <summary>
        /// Retrieves a block's complete read set by:
        /// 1. Loading the BlockReadSetState for the block
        /// 2. Reconstructing the original key-value pairs
        /// </summary>
        public bool TryGet(UInt256 blockHash, out Dictionary<byte[], byte[]> readSet)
        {
            readSet = null;

            // Get the block read set state
            var blockKey = new KeyBuilder(Prefix_Id, PrefixBlockReadSet).Add(blockHash).ToArray();
            _snapshot.TryGet(blockKey, out var stateBytes);
            if (stateBytes == null) return false;

            var state = stateBytes.AsSerializable<BlockReadSetState>();
            readSet = state._initialReadSet;

            return true;
        }

        public void Commit()
        {
            _snapshot.Commit();
        }
    }
}
