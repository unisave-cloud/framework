namespace Unisave.Serialization.Context
{
    /// <summary>
    /// Reason for serialization
    /// </summary>
    public enum SerializationReason
    {
        /// <summary>
        /// The data needs to be transmitted over a network,
        /// within the Unisave world.
        /// (e.g. a facet call or broadcasting)
        /// (this also includes short-term storage, like cache or session)
        /// </summary>
        Transmission = 1,
        
        /// <summary>
        /// The data needs to be stored somewhere long-term,
        /// within the Unisave world.
        /// (e.g. in a database or a file)
        /// </summary>
        Storage = 2,
    }
}