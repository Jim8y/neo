using Microsoft.Extensions.Configuration;
using Neo.Persistence;
using System;

namespace Neo.Plugins.StatelessBlock
{
    class Settings
    {
        /// <summary>
        /// Maximum number of read sets to keep in memory cache
        /// </summary>
        public uint MaxCachedReadSets { get; }

        /// <summary>
        /// Path to store read sets, relative to the chain path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Storage provider for read sets (e.g. RocksDBStore, LevelDBStore)
        /// If not specified, will use the same provider as the main chain
        /// </summary>
        public string Provider { get; }

        public static Settings Default { get; private set; }

        private Settings(IConfigurationSection section)
        {
            MaxCachedReadSets = section.GetValue("MaxCachedReadSets", 1000u);
            Path = section.GetValue("Path", "ReadSets_{0}");
            Provider = section.GetValue("Provider", string.Empty);
        }

        public static void Load(IConfigurationSection section)
        {
            Default = new Settings(section);
        }
    }
}
