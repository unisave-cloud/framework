namespace Unisave.Bootstrapping
{
    /// <summary>
    /// A bootstrapper is a class that is instantiated and run during
    /// Unisave Framework startup. It is analogous to the "Program.Main()"
    /// method, but is designed to be modular. Each backend module can have
    /// its own bootstrapper that registers services into the service container
    /// and prepares the module according to the provided configuration.
    /// Bootstrapper dependencies should be injected via its constructor.
    /// </summary>
    public interface IBootstrapper
    {
        /// <summary>
        /// This is like the "Program.Main()" method in console applications,
        /// it is executed when the bootstrapper is meant to run.
        /// </summary>
        void Main();
    }
}