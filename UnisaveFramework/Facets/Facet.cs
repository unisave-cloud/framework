using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LightJson;
using Unisave.Foundation;
using Unisave.Serialization;
using Unisave.Serialization.Context;
using Unisave.Utils;

namespace Unisave.Facets
{
    public abstract class Facet
    {
        /// <summary>
        /// Tries to find given facet name inside a collection of types
        /// Throws FacetSearchException on failure
        /// </summary>
        /// <param name="facetName">Name of the facet to find</param>
        /// <param name="types">Domain to search through</param>
        /// <returns>Requested facet type</returns>
        public static Type FindFacetTypeByName(
            string facetName,
            IEnumerable<Type> types
        )
        {
            // === try to find by exact FullName ===
            
            List<Type> facetCandidates = types
                .Where(t => t.FullName == facetName)
                .Where(t => typeof(Facet).IsAssignableFrom(t))
                .Where(t => t != typeof(Facet)) // except for the abstract class itself
                .ToList();
            
            if (facetCandidates.Count > 0)
                return facetCandidates[0];
            
            // === try to find just by Name ===
            
            facetCandidates = types
                .Where(t => t.Name == facetName)
                .Where(t => typeof(Facet).IsAssignableFrom(t))
                .Where(t => t != typeof(Facet)) // except for the abstract class itself
                .ToList();
            
            if (facetCandidates.Count > 1)
                throw new FacetSearchException(
                    $"Facet name '{facetName}' is ambiguous. "
                    + "Make sure you don't have two facets with the same name."
                );

            if (facetCandidates.Count == 0)
                throw new FacetSearchException(
                    $"Facet '{facetName}' was not found. "
                    + $"Make sure your class inherits from the {nameof(Facet)} class."
                );

            return facetCandidates[0];
        }
        
        /// <summary>
        /// Creates facet instance of given type
        /// </summary>
        public static Facet CreateInstance(Type facetType, IContainer services)
        {
            // check proper parent
            if (!typeof(Facet).IsAssignableFrom(facetType))
                throw new InstantiationException(
                    $"Provided type {facetType} does not inherit from "
                    + $"the {typeof(Facet)} class."
                );

            // let the ioc container create the instance
            return (Facet) services.Resolve(facetType);
        }
        
        /// <summary>
        /// Finds a specific method by it's name.
        /// Throws MethodSearchException in case of ambiguity or other problems.
        /// </summary>
        public static MethodInfo FindMethodByName(Type type, string name)
        {
            // non-public as well to print an error
            BindingFlags flags = BindingFlags.Instance
                                 | BindingFlags.Public
                                 | BindingFlags.NonPublic;
            
            List<MethodInfo> methods = type.GetMethods(flags)
                .Where(m => m.Name == name)
                .ToList();

            if (methods.Count > 1)
                throw new MethodSearchException(
                    $"Class '{type}' has multiple methods called '{name}'. "
                    + "Note that Unisave does not support method overloading"
                    + " for facets. Also make sure you aren't using default "
                    + "values for some arguments."
                );

            if (methods.Count == 0)
                throw new MethodSearchException(
                    $"Class '{type}' doesn't have public method "
                    + $"called '{name}'."
                );

            MethodInfo methodInfo = methods[0];

            if (!methodInfo.IsPublic)
                throw new MethodSearchException(
                    $"Method '{type}.{name}' has to be public in "
                    + "order to be called remotely."
                );
            
            if (IsAsyncVoid(methodInfo))
                throw new MethodSearchException(
                    $"Method '{type}.{name}' cannot be declared as "
                    + "'async void'. Use 'async Task' instead."
                );

            return methodInfo;
        }

        /// <summary>
        /// Identifies an 'async void' method.
        /// </summary>
        private static bool IsAsyncVoid(MethodInfo methodInfo)
        {
            // https://stackoverflow.com/questions/30782332/
            // identify-an-async-void-method-through-reflection

            // method must be void
            if (methodInfo.ReturnType != typeof(void))
                return false;
            
            // and it must be marked with the attribute
            AsyncStateMachineAttribute attribute = methodInfo
                .GetCustomAttribute<AsyncStateMachineAttribute>();
            if (attribute == null)
                return false;

            // if both conditions hold, we have an 'async void' method here
            return true;
        }
        
        /// <summary>
        /// Serializes arguments for a facet method call
        /// NOTE: used by the client code
        /// </summary>
        public static JsonArray SerializeArguments(
            MethodInfo methodInfo,
            object[] arguments
        )
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length != arguments.Length)
                throw new ArgumentException(
                    $"Given arguments array has different length than the " +
                    $"number of parameters the method accepts."
                );
            
            var jsonArgs = new JsonArray();

            for (int i = 0; i < parameters.Length; i++)
            {
                jsonArgs.Add(
                    Serializer.ToJson(
                        arguments[i],
                        parameters[i].ParameterType,
                        SerializationContext.ClientToServer
                    )
                );
            }
            
            return jsonArgs;
        }
        
        /// <summary>
        /// Deserializes arguments for a method
        /// Skipped arguments will have null value and you can assign them later
        /// Throws ExecutionSerializationException
        /// </summary>
        /// <param name="jsonArguments"></param>
        /// <param name="skip">How many first arguments to skip</param>
        /// <param name="methodInfo"></param>
        public static object[] DeserializeArguments(
            MethodInfo methodInfo,
            JsonArray jsonArguments,
            int skip = 0
        )
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length != jsonArguments.Count)
                throw new UnisaveSerializationException(
                    $"Method '{methodInfo.DeclaringType?.Name}.{methodInfo.Name}'"
                    + " accepts different number of arguments than provided. "
                    + "Make sure you don't use the params keyword or "
                    + "default argument values, "
                    + "since it is not supported by Unisave."
                );

            object[] deserializedArguments = new object[parameters.Length];

            for (int i = skip; i < parameters.Length; i++)
            {
                deserializedArguments[i] = Serializer.FromJson(
                    jsonArguments[i],
                    parameters[i].ParameterType,
                    DeserializationContext.ClientToServer
                );
            }

            return deserializedArguments;
        }

        /// <summary>
        /// Serializes the value returned from a facet
        /// </summary>
        /// <param name="returnType">Type of the return value as declared by the facet</param>
        /// <param name="value">Returned value</param>
        /// <returns></returns>
        public static JsonValue SerializeReturnedValue(
            Type returnType,
            object value
        )
        {
            return Serializer.ToJson(
                value,
                returnType,
                SerializationContext.ServerToClient
            );
        }
    }
}
