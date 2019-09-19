using System.Reflection;

namespace Unisave
{
    /// <summary>
    /// Collection of metadata about the Unisave framework
    /// </summary>
    public static class FrameworkMeta
    {
        /// <summary>
        /// Version of the framework
        /// </summary>
        public static string Version => typeof(FrameworkMeta).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;
    }
}