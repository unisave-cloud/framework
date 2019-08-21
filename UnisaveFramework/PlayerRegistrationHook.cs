using System;
using System.Collections.Generic;
using Unisave.Runtime;
using Unisave.Serialization;
using Unisave.Exceptions;
using LightJson;

namespace Unisave
{
    /// <summary>
    /// Base class for player registration hooks
    /// </summary>
    public abstract class PlayerRegistrationHook
    {
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
        /// Arguments provided during registration
        /// </summary>
        private Dictionary<string, JsonValue> arguments;

        /// <summary>
        /// Creates instance of a hook
        /// </summary>
        public static PlayerRegistrationHook CreateInstance(
            Type hookType, UnisavePlayer player, Dictionary<string, JsonValue> arguments
        )
        {
            // create instance
            PlayerRegistrationHook hook = ExecutionHelper.Instantiate<PlayerRegistrationHook>(hookType);

            // assign properties
            hook.Player = player;
            hook.arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));

            return hook;
        }

        /// <summary>
        /// Execute the actual hook
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// Rejects the registration if one of the listed arguments is missing
        /// </summary>
        protected void ValidateArgumentsProvided(params string[] names)
        {
            foreach (string name in names)
            {
                if (!arguments.ContainsKey(name))
                    Reject($"Hook {this.GetType().Name} expects a parameter: {name}");
            }
        }

        /// <summary>
        /// Immediately stops code execution and rejects player registration
        /// </summary>
        /// <param name="message">Message for the player</param>
        /// <param name="payload">Optional payload for the client side</param>
        protected void Reject(string message = null, object payload = null)
        {
            throw new PlayerRegistrationRejectedException(message, payload);
        }

        /// <summary>
        /// Returns an argument given during registration.
        /// </summary>
        /// <param name="name">Name of the argument</param>
        /// <param name="defaultValue">Value to return when argument missing</param>
        /// <typeparam name="T">Type of the argument</typeparam>
        protected T GetArgument<T>(string name, T defaultValue = default(T))
        {
            if (!arguments.ContainsKey(name))
                return defaultValue;

            return (T) Loader.Load(arguments[name], typeof(T));
        }
    }
}
