namespace Unisave.Serialization.Context
{
    /// <summary>
    /// Reason for serialization
    /// </summary>
    public enum SerializationReason
    {
        /// <summary>
        /// The data only needs to be transferred over a network
        /// (e.g. a facet call or broadcasting)
        /// (this also includes short-term storage, like cache or session)
        /// </summary>
        Transfer = 0,
        
        /// <summary>
        /// The data needs to be stored somewhere long-term
        /// (e.g. in a database or a file)
        /// </summary>
        Storage = 1
    }
}