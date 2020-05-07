namespace Unisave.Arango
{
    /// <summary>
    /// Types of arango indices
    /// </summary>
    public class IndexType
    {
        /// <summary>
        /// Persistent index
        /// https://www.arangodb.com/docs/stable/indexing-persistent.html
        /// </summary>
        public const string Persistent = "persistent";
        
        /// <summary>
        /// Time-to-live index
        /// https://www.arangodb.com/docs/stable/indexing-ttl.html
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const string TTL = "ttl";
    }
}