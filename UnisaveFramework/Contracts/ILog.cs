using LightJson;
using Unisave.Logging;

namespace Unisave.Contracts
{
    /// <summary>
    /// Interface for logging
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Critical conditions, should be resolved as soon as possible.
        /// </summary>
        void Critical(string message, object context = null);
        
        /// <summary>
        /// Something didn't go as planned, but is not very severe.
        /// </summary>
        void Error(string message, object context = null);
        
        /// <summary>
        /// Exceptional occurrences that are not errors
        /// </summary>
        void Warning(string message, object context = null);
        
        /// <summary>
        /// Interesting events
        /// </summary>
        void Info(string message, object context = null);
        
        /// <summary>
        /// Log a message with given log level and context object
        /// </summary>
        void LogMessage(LogLevel level, string message, object context = null);
    }
}