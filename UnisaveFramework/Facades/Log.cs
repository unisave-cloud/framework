using System;
using Unisave.Contracts;
using Unisave.Logging;

namespace Unisave.Facades
{
    /// <summary>
    /// Facade for logging messages
    /// </summary>
    public static class Log
    {
        private static ILog GetLog()
        {
            if (!Facade.HasApp)
                return new ClientSideLog();
            
            return Facade.App.Services.Resolve<ILog>();
        }
        
        /// <summary>
        /// Critical conditions, should be resolved as soon as possible.
        /// </summary>
        public static void Critical(string message, object context = null)
            => GetLog().Critical(message, context);
        
        /// <summary>
        /// Something didn't go as planned, but is not very severe.
        /// </summary>
        public static void Error(string message, object context = null)
            => GetLog().Error(message, context);
        
        /// <summary>
        /// Exceptional occurrences that are not errors
        /// </summary>
        public static void Warning(string message, object context = null)
            => GetLog().Warning(message, context);
        
        /// <summary>
        /// Interesting events
        /// </summary>
        public static void Info(string message, object context = null)
            => GetLog().Info(message, context);
        
        /// <summary>
        /// Log a message with given log level and context object
        /// </summary>
        public static void LogMessage(LogLevel level, string message, object context = null)
            => GetLog().LogMessage(level, message, context);
    }
}