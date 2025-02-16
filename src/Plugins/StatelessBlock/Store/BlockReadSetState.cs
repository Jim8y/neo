using Neo.Extensions;
using Neo.IO;

namespace Neo.Plugins.StatelessBlock.Store.States
{
    /// <summary>
    /// BlockReadSetState represents the minimal storage state required for a block's execution.
    /// It stores only the first read value for each storage key during block execution,
    /// ignoring subsequent modifications.
    /// </summary>
    public class BlockReadSetState : ISerializable
    {
        /// <summary>
        /// Maps storage keys to their initial values read during block execution
        /// </summary>
        public Dictionary<byte[], byte[]> _initialReadSet = new();

        public int Size => _initialReadSet?.Count ?? 0;

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(_initialReadSet.Count);
            foreach (var (key, value) in _initialReadSet)
            {
                writer.WriteVarBytes(key);
                writer.WriteVarBytes(value);
            }
        }

        public void Deserialize(ref MemoryReader reader)
        {
            _initialReadSet = new Dictionary<byte[], byte[]>();
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadVarMemory();
                var value = reader.ReadVarMemory();
                _initialReadSet[key.ToArray()] = value.ToArray();
            }
        }
    }
}
