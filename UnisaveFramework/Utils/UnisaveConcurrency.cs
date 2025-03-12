using System;
using System.Threading.Tasks;

namespace Unisave.Utils
{
    /// <summary>
    /// Utility functions for working with concurrency in the context of the
    /// Unisave runtime environment.
    /// </summary>
    public static class UnisaveConcurrency
    {
        /// <summary>
        /// Invokes the given task launcher callback and synchronously waits
        /// for the task to complete. This blocks the current thread until
        /// the task finishes. The task launcher and the awaiting is performed
        /// in some thread of the default thread pool, not the calling thread.
        /// This is to prevent deadlocks in single-threaded async execution
        /// configuration of the unisave runtime environment.
        ///
        /// Learn more here:
        /// https://github.com/unisave-cloud/worker/blob/master/docs/deadlocks.md
        /// </summary>
        /// <param name="taskLauncher">
        /// Callback that creates the thread instance. You MUST create the
        /// task here. You MUST NOT create it earlier and just pass it here,
        /// otherwise you get a deadlock.
        /// </param>
        /// <typeparam name="TReturn">
        /// Return type for the task and this method
        /// </typeparam>
        /// <returns>
        /// Returns what the task returns when finishes.
        /// (or throws what the task throws)
        /// </returns>
        /// <example>
        /// This is how you convert async code to sync blocking code:
        /// <code>
        /// // taking this:
        /// var result = await MyAsyncMethod();
        /// 
        /// // you rewrite it like this:
        /// var result = UnisaveConcurrency.WaitForTask(
        ///     () => MyAsyncMethod()
        /// );
        /// 
        /// // or if no arguments are given, you can do just
        /// var result = UnisaveConcurrency.WaitForTask(MyAsyncMethod)
        /// </code>
        /// </example>
        public static TReturn WaitForTask<TReturn>(
            Func<Task<TReturn>> taskLauncher
        )
        {
            return Task.Run(
                taskLauncher
            ).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Invokes the given task launcher callback and synchronously waits
        /// for the task to complete. This blocks the current thread until
        /// the task finishes. The task launcher and the awaiting is performed
        /// in some thread of the default thread pool, not the calling thread.
        /// This is to prevent deadlocks in single-threaded async execution
        /// configuration of the unisave runtime environment.
        ///
        /// Learn more here:
        /// https://github.com/unisave-cloud/worker/blob/master/docs/deadlocks.md
        /// </summary>
        /// <param name="taskLauncher">
        /// Callback that creates the thread instance. You MUST create the
        /// task here. You MUST NOT create it earlier and just pass it here,
        /// otherwise you get a deadlock.
        /// </param>
        /// <returns>
        /// Returns what the task returns when finishes.
        /// (or throws what the task throws)
        /// </returns>
        /// <example>
        /// This is how you convert async code to sync blocking code:
        /// <code>
        /// // taking this:
        /// var result = await MyAsyncMethod();
        /// 
        /// // you rewrite it like this:
        /// var result = UnisaveConcurrency.WaitForTask(
        ///     () => MyAsyncMethod()
        /// );
        /// 
        /// // or if no arguments are given, you can do just
        /// var result = UnisaveConcurrency.WaitForTask(MyAsyncMethod)
        /// </code>
        /// </example>
        public static void WaitForTask(
            Func<Task> taskLauncher
        )
        {
            Task.Run(
                taskLauncher
            ).GetAwaiter().GetResult();
        }
    }
}