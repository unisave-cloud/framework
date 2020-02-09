using System.Collections.Generic;

namespace Unisave.Arango.Emulation
{
    /// <summary>
    /// Simulates ArangoDB database in memory
    /// </summary>
    public class ArangoInMemory
    {
        public readonly Dictionary<string, Collection> collections;
    }
}