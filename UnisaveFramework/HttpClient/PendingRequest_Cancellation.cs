using System;
using System.Threading;

namespace Unisave.HttpClient
{
    public partial class PendingRequest
    {
        /// <summary>
        /// Cancellation token for the request
        /// </summary>
        private CancellationToken cancellationToken = CancellationToken.None;
        
        /// <summary>
        /// Maximum amount of time to wait for the response
        /// (defaults to 10 seconds)
        /// </summary>
        private TimeSpan timeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Sets the cancellation token that cancels this request
        /// </summary>
        /// <param name="token"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithCancellation(CancellationToken token)
        {
            cancellationToken = token;
            return this;
        }
        
        /// <summary>
        /// Sets the maximum number of seconds to wait for a response.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithTimeout(double seconds)
        {
            timeout = TimeSpan.FromSeconds(seconds);
            return this;
        }
        
        /// <summary>
        /// Sets the maximum number of time to wait for a response.
        /// </summary>
        /// <param name="time"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithTimeout(TimeSpan time)
        {
            timeout = time;
            return this;
        }

        /// <summary>
        /// Creates a linked cancellation token source, that aggregates the
        /// timeout token with the user cancellation token. The returned
        /// CTS must be disposed.
        /// </summary>
        private CancellationTokenSource CreateLinkedCts()
        {
            var timeoutCts = new CancellationTokenSource();
            timeoutCts.CancelAfter(timeout);

            return CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutCts.Token
            );
        }
    }
}