using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using LightJson;
using Unisave.Exceptions;

namespace Unisave.Runtime
{
    public static class PlayerRegistrationHookCall
    {
        public static JsonObject Start(JsonObject executionParameters, Type[] gameAssemblyTypes)
        {
            // extract arguments
            JsonArray jsonArguments = executionParameters["arguments"];
            string playerId = executionParameters["playerId"];
            UnisavePlayer player = new UnisavePlayer(playerId);

            // find all hooks
            List<PlayerRegistrationHook> hooks = gameAssemblyTypes
                .Where(t => typeof(PlayerRegistrationHook).IsAssignableFrom(t))
                .Select(t => PlayerRegistrationHook.CreateInstance(t, player))
                .ToList();

            hooks.Sort((a, b) => a.Order - b.Order); // small to big

            try
            {
                foreach (var hook in hooks)
                {
                    ExecutionHelper.ExecuteMethod(
                        hook,
                        PlayerRegistrationHook.HookMethodName,
                        jsonArguments,
                        out MethodInfo methodInfo
                    );
                }
            }
            catch (MethodSearchException e)
            {
                throw new InvalidMethodParametersException(e);
            }
            catch (ExecutionSerializationException e)
            {
                throw new InvalidMethodParametersException(e);
            }
            catch (TargetInvocationException e)
            {
                throw new GameScriptException(e.InnerException);
            }

            // response is an empty json object
            return new JsonObject();
        }
    }
}
