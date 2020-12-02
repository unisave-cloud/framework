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
        /// Returns the parametrized string name for a given channel
        /// </summary>
        /// <param name="parameters">Parameters for the channel</param>
        /// <typeparam name="TChannel">Channel type</typeparam>
        /// <returns></returns>
        public static string GetStringName<TChannel>(params string[] parameters)
            where TChannel : BroadcastingChannel, new()
        {
            var jsonParams = new JsonArray();
            foreach (string p in parameters)
                jsonParams.Add(p);

            return typeof(TChannel).FullName + jsonParams.ToString();
        }
    }
}