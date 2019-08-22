using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Unisave.Exceptions;
using LightJson;
using Unisave.Serialization;

namespace Unisave.Runtime
{
    /// <summary>
    /// Extracts commonly used procedures when executing game scripts
    /// </summary>
    public static class ExecutionHelper
    {
        /// <summary>
        /// Creates concrete instance of some type that inherits from an abstract ancestor
        /// The concrete type has to have parameterless contructor
        /// Example: Concrete facet instantiation, but returned as an abstract Facet
        /// </summary>
        public static TAbstract Instantiate<TAbstract>(Type concreteType)
        {
            // check proper parent
            if (!typeof(TAbstract).IsAssignableFrom(concreteType))
                throw new InstantiationException(
                    $"Provided type {concreteType} does not inherit from the {typeof(TAbstract)} class."
                );

            // get parameterless constructor
            ConstructorInfo ci = concreteType.GetConstructor(new Type[] { });

            if (ci == null)
                throw new InstantiationException(
                    $"Provided type {concreteType} lacks parameterless constructor."
                );

            // create instance
            return (TAbstract)ci.Invoke(new object[] { });
        }

        /// <summary>
        /// Executes a method, handles method search and serialization.
        /// Throws MethodSearchException, ExecutionSerializationException, TargetInvocationException
        /// Inner exceptions are automatically wrapped inside GameScriptException
        /// </summary>
        public static JsonValue ExecuteMethod(
            object instance, string methodName, JsonArray arguments, out MethodInfo methodInfo
        )
        {
            // find the requested method
            methodInfo = FindMethodByName(instance.GetType(), methodName);

            // deserialize arguments
            object[] deserializedArguments = DeserializeArguments(methodInfo, arguments);

            // call the facet method
            // RAISES: TargetInvocationException
            object returnedValue = methodInfo.Invoke(instance, deserializedArguments);

            // if returned void
            if (methodInfo.ReturnType == typeof(void))
                return JsonValue.Null;

            // serialize returned value
            return SerializeReturnValue(methodInfo, returnedValue);
        }

        /// <summary>
        /// Finds a specific method by it's name.
        /// Throws MethodSearchException in case of ambiguity or other problems.
        /// </summary>
        public static MethodInfo FindMethodByName(Type type, string name)
        {
            List<MethodInfo> methods = type.GetMethods(
                BindingFlags.Instance | BindingFlags.DeclaredOnly
                    | BindingFlags.Public | BindingFlags.NonPublic // non-public as well to print an error
            )
                .Where(m => m.Name == name)
                .ToList();

            if (methods.Count > 1)
                throw new MethodSearchException(
                    $"Class '{type}' has multiple methods called '{name}'. "
                    + "Note that Unisave does not support method overloading for RPCs. "
                    + "Also make sure you aren't using default values for some arguments."
                );

            if (methods.Count == 0)
                throw new MethodSearchException(
                    $"Class '{type}' doesn't have public method called '{name}'."
                );

            MethodInfo methodInfo = methods[0];

            if (!methodInfo.IsPublic)
                throw new MethodSearchException(
                    $"Method '{type}.{name}' has to be public in order to be called remotely."
                );

            return methodInfo;
        }

        /// <summary>
        /// Deserializes arguments for a method
        /// Skipped arguments will have null value and you can assign them later
        /// Throws ExecutionSerializationException
        /// </summary>
        /// <param name="skip">How many first arguments to skip</param>
        public static object[] DeserializeArguments(MethodInfo methodInfo, JsonArray jsonArguments, int skip = 0)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters(); // NOT including the "this" parameter

            if (parameters.Length != jsonArguments.Count)
                throw new ExecutionSerializationException(
                    $"Method '{methodInfo.DeclaringType.Name}.{methodInfo.Name}' accepts different number of arguments than provided. "
                    + "Make sure you don't use the params keyword or default argument values, "
                    + "since it is not supported by Unisave."
                );

            object[] deserializedArguments = new object[parameters.Length];

            for (int i = skip; i < parameters.Length; i++)
            {
                try
                {
                    deserializedArguments[i] = Serializer.FromJson(jsonArguments[i], parameters[i].ParameterType);
                }
                catch (Exception e)
                {
                    throw new ExecutionSerializationException(
                        $"Exception occured when deserializing the argument '{parameters[i].Name}'", e
                    );
                }
            }

            return deserializedArguments;
        }

        /// <summary>
        /// Serializes value returned by some method
        /// Throws ExecutionSerializationException
        /// </summary>
        public static JsonValue SerializeReturnValue(MethodInfo methodInfo, object value)
        {
            try
            {
                return Serializer.ToJson(value);
            }
            catch (Exception e)
            {
                throw new ExecutionSerializationException(
                    "Exception occured when serializing value "
                    + $"returned from '{methodInfo.DeclaringType.Name}.{methodInfo.Name}'", e
                );
            }
        }
    }
}
