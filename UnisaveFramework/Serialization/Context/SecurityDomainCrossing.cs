namespace Unisave.Serialization.Context
{
    /// <summary>
    /// Does the serialized data cross a security domain boundary?
    /// </summary>
    public enum SecurityDomainCrossing
    {
        /// <summary>
        /// The data stays in the same domain
        /// (on the server, or outside the server)
        /// </summary>
        NoCrossing = 0,
        
        /// <summary>
        /// The data leaves the server,
        /// sensitive information should be forgotten
        /// </summary>
        LeavingServer = 1,
        
        /// <summary>
        /// The data enters the server,
        /// should be treated with caution
        /// </summary>
        EnteringServer = 2
    }
}