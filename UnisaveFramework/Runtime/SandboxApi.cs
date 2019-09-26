using System;

namespace Unisave.Runtime
{
    /// <summary>
    /// Communication API between the framework and the sandbox
    ///
    /// The API is this abstract because a given sandbox version may
    /// run multiple framework versions.
    /// </summary>
    public static class SandboxApi
    {
        /// <summary>
        /// Framework sends a message to the sandbox
        /// string: channel name
        /// int: message type
        /// byte[]: message content
        /// byte[]: response to the message
        /// </summary>
        public static Func<string, int, byte[], byte[]> SendMessage = null;

        /// <summary>
        /// Sandbox sends message to the framework
        /// PLANNED FEATURE, CURRENTLY NOT USED
        /// </summary>
        public static Func<string, int, byte[], byte[]> MessageReceived = null;
    }
}