using System.Collections.Generic;

namespace Unisave.Arango.Database
{
    /// <summary>
    /// Simulates ArangoDB database in memory
    /// </summary>
    public class InMemoryArango
    {
        public readonly Dictionary<string, Collection> collections;
    }
}