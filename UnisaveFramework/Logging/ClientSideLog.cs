using System.ComponentModel;
using Unisave.Contracts;
using UnityEngine;

namespace Unisave.Logging
{
    /// <summary>
    /// This logger implementation is used by the Log facade on the client side
    /// (when no application instance is set for the facades)
    /// </summary>
    internal class ClientSideLog : ILog
    {
        public void Critical(string message, object context = null)
        {
            Debug.LogError("[Critical] " + message);
        }

        public void Error(string message, object context = null)
        {
            Debug.LogError(message);
        }

        public void Warning(string message, object context = null)
        {
            Debug.LogWarning(message);
        }

        public void Info(string message, object context = null)
        {
            Debug.Log(message);
        }

        public void LogMessage(LogLevel level, string message, object context = null)
        {
            switch (level)
            {
                case LogLevel.Info:
                    Info(message, context);
                    break;
                
                case LogLevel.Warning:
                    Warning(message, context);
                    break;
                
                case LogLevel.Error:
                    Error(message, context);
                    break;
                
                case LogLevel.Critical:
                    Critical(message, context);
                    break;
                
                default:
                    throw new InvalidEnumArgumentException(nameof(level));
            }
        }
    }
}