namespace Unisave.Bootstrapping
{
    /// <summary>
    /// Definitions of well-known bootstrapping stage numbers
    /// </summary>
    public static class BootstrappingStage
    {
        /// <summary>
        /// The stage in which Unisave Framework bootstrappers are run
        /// </summary>
        public static readonly int Framework = -1_000;
        
        /// <summary>
        /// The stage in which official and third-party modules
        /// (unisave extensions) are run
        /// </summary>
        public static readonly int Modules = -500;
        
        /// <summary>
        /// The default stage, in which normal user's bootstrappers run
        /// </summary>
        public static readonly int Default = 0;
        
        /// <summary>
        /// Late stage that runs even after the user's bootstrappers,
        /// may be used for some module's post-configuration of the user's setup
        /// </summary>
        public static readonly int Late = 1_000;
    }
}