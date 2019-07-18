using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Unisave.Exceptions;

namespace Unisave
{
    public abstract class Facet
    {
        /// <summary>
        /// The player who called this facet
        /// </summary>
        protected UnisavePlayer Caller { get; private set; }

        /// <summary>
        /// Creates facet instance of given type
        /// </summary>
        public static Facet CreateInstance(Type facetType, UnisavePlayer caller)
        {
            // check proper parent
            if (!typeof(Facet).IsAssignableFrom(facetType))
                throw new FacetInstantiationException(
                    $"Provided type {facetType} does not inherit from the Facet class."
                );

            // get parameterless constructor
            ConstructorInfo ci = facetType.GetConstructor(new Type[] { });

            if (ci == null)
                throw new FacetInstantiationException(
                    $"Provided facet type {facetType} lacks parameterless constructor."
                );

            // create instance
            Facet facet = (Facet)ci.Invoke(new object[] { });

            // assign properties
            facet.Caller = caller;

            return facet;
        }

        /// <summary>
        /// Tries to find given facet name inside a collection of types
        /// Throws FacetSearchException on failure
        /// </summary>
        /// <param name="facetName">Name of the facet to find</param>
        /// <param name="types">Domain to search through</param>
        /// <returns>Requested facet type</returns>
        public static Type FindFacetTypeByName(string facetName, IEnumerable<Type> types)
        {
            List<Type> facetCandidates = types
                .Where(t => t.Name == facetName)
                .Where(t => typeof(Facet).IsAssignableFrom(t))
                .ToList();

            if (facetCandidates.Count > 1)
                throw new FacetSearchException(
                    $"Facet name '{facetName}' is ambiguous. "
                    + "Make sure you don't have two facets with the same name.",
                    FacetSearchException.ProblemType.FacetNameAmbiguous
                );

            if (facetCandidates.Count == 0)
                throw new FacetSearchException(
                    $"Facet '{facetName}' was not found. "
                    + $"Make sure your class inherits from the {nameof(Unisave.Facet)} class.",
                    FacetSearchException.ProblemType.FacetNotFound
                );

            return facetCandidates[0];
        }

        /// <summary>
        /// Tries to find given facet method inside a facet type
        /// Throws FacetMethodSearchException on failure
        /// </summary>
        /// <param name="facetType">Facet type to search through</param>
        /// <param name="methodName">Name of the requested method</param>
        public static MethodInfo FindFacetMethodByName(Type facetType, string methodName)
        {
            List<MethodInfo> methods = facetType.GetMethods(
                BindingFlags.Instance | BindingFlags.DeclaredOnly
                    | BindingFlags.Public | BindingFlags.NonPublic // non-public as well to print an error
            )
                .Where(m => m.Name == methodName)
                .ToList();

            if (methods.Count > 1)
                throw new FacetMethodSearchException(
                    $"Facet '{facetType}' has multiple methods called '{methodName}'. "
                    + "Note that Unisave does not support method overloading for facets. "
                    + "Also make sure you aren't using default values for some arguments.",
                    FacetMethodSearchException.ProblemType.MethodNameAmbiguous
                );

            if (methods.Count == 0)
                throw new FacetMethodSearchException(
                    $"Facet '{facetType}' doesn't have public method called '{methodName}'.",
                    FacetMethodSearchException.ProblemType.MethodDoesNotExist
                );

            MethodInfo methodInfo = methods[0];

            if (!methodInfo.IsPublic)
                throw new FacetMethodSearchException(
                    $"Method '{facetType}.{methodName}' has to be public in order to be called remotely.",
                    FacetMethodSearchException.ProblemType.MethodNotPublic
                );

            return methodInfo;
        }
    }
}
