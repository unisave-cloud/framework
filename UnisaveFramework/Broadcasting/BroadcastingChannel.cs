using System;
using LightJson;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// Represents a channel though which messages can be broadcast
    /// </summary>
    public abstract class BroadcastingChannel
    {
        /// <summary>
        /// How many parameters does the channel have
        /// </summary>
        public virtual int ParameterCount => 0;
        
        /// <summary>
        /// Called to determine whether a client can join a channel
        /// </summary>
        /// <param name="parameters">Parameters for the channel</param>
        /// <returns>True if the client is allowed to join</returns>
        public virtual bool Join(params string[] parameters)
        {
            // nobody can join a channel by default
            return false;
        }

        public static void ValidateParameters<TChannel>(
            params string[] parameters
        ) where TChannel : BroadcastingChannel, new()
        {
            if (parameters == null)
                throw new ArgumentException(
                    "Broadcasting channel parameters cannot be null."
                );
            
            var channel = new TChannel();
            
            if (parameters.Length != channel.ParameterCount)
                throw new ArgumentException(
                    $"Channel {typeof(TChannel)} expects {channel.ParameterCount} " +
                    $"parameters but {parameters.Length} were given."
                );
        }

        /// <summary>
        /// Returns the parametrized string name for a given channel
        /// </summary>
        /// <param name="parameters">Parameters for the channel</param>
        /// <typeparam name="TChannel">Channel type</typeparam>
        /// <returns></returns>
        public static string GetStringName<TChannel>(params string[] parameters)
            where TChannel : BroadcastingChannel, new()
        {
            ValidateParameters<TChannel>(parameters);
            
            var jsonParams = new JsonArray();
            foreach (string p in parameters)
                jsonParams.Add(p);

            return typeof(TChannel).FullName + jsonParams.ToString();
        }
    }
}