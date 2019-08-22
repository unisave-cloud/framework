using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using LightJson;
using Unisave.Exceptions;
using Unisave.Serialization;

namespace Unisave.Runtime
{
    public static class PlayerRegistrationHookCall
    {
        public static JsonObject Start(JsonObject executionParameters, Type[] gameAssemblyTypes)
        {
            // extract arguments
            JsonObject jsonArguments = executionParameters["arguments"];
            string playerId = executionParameters["playerId"];
            UnisavePlayer player = new UnisavePlayer(playerId);

            Dictionary<string, JsonValue> arguments = (Dictionary<string, JsonValue>) Serializer.FromJson(
                jsonArguments,
                typeof(Dictionary<string, JsonValue>)
            );

            // find all hooks
            List<PlayerRegistrationHook> hooks = gameAssemblyTypes
                .Where(t => typeof(PlayerRegistrationHook).IsAssignableFrom(t))
                .Select(t => PlayerRegistrationHook.CreateInstance(t, player, arguments))
                .ToList();

            hooks.Sort((a, b) => a.Order - b.Order); // small to big

            try
            {
                foreach (var hook in hooks)
                    hook.Run();
            }
            catch (Exception e)
            {
                throw new GameScriptException(e);
            }

            // response is an empty json object
            return new JsonObject();
        }
    }
}
