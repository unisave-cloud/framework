using System;
using Unisave.Runtime;

namespace Unisave
{
    /// <summary>
    /// Base class for player registration hooks
    /// </summary>
    public abstract class PlayerRegistrationHook
    {
        /// <summary>
        /// Name of the method that's called as the hook
        /// </summary>
        public const string HookMethodName = "Run";

        /// <summary>
        /// In what order to call all the hooks
        /// (from smallest to greatest)
        /// </summary>
        public virtual int Order => 1;

        /// <summary>
        /// Player that is being registered
        /// </summary>
        protected UnisavePlayer Player { get; private set; }

        /// <summary>
        /// Creates instance of a hook
        /// </summary>
        public static PlayerRegistrationHook CreateInstance(Type hookType, UnisavePlayer player)
        {
            // create instance
            PlayerRegistrationHook hook = ExecutionHelper.Instantiate<PlayerRegistrationHook>(hookType);

            // assign properties
            hook.Player = player;

            return hook;
        }
    }
}
