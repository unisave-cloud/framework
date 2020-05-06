using System;
using System.Runtime.Serialization;

namespace Unisave.Exceptions
{
    /// <summary>
    /// Thrown when there's a problem with configuration - meaning Unisave
    /// cannot work because it cannot load configuration or configuration
    /// is nonsensical.
    ///
    /// By configuration is meant anything that tells the framework how to
    /// set itself up before doing the work that's required.
    ///
    /// This exception should be thrown when there's a problem inside
    /// a ServiceProvider, while registering services into the IoC container.
    ///
    /// Few examples would be:
    /// - requesting unknown session driver
    /// - setting session lifetime to negative value
    /// - missing required configuration value
    /// </summary>
    [Serializable]
    public class ConfigurationException : UnisaveException
    {
        public ConfigurationException() : base(
            "Unisave Framework booting failed due to invalid configuration."
        ) { }

        public ConfigurationException(string message) : base(message)
        {
        }

        public ConfigurationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ConfigurationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}