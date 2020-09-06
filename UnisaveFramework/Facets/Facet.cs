using System;
using System.Collections.Generic;
using System.Linq;
using Unisave.Exceptions;
using Unisave.Runtime.Kernels;
using Unisave.Utils;

namespace Unisave.Facets
{
    public abstract class Facet
    {
        /// <summary>
        /// Creates facet instance of given type
        /// </summary>
        public static Facet CreateInstance(Type facetType)
        {
            Facet facet = ExecutionHelper.Instantiate<Facet>(facetType);
            
            // Here I can assign additional values if needed in the future
            
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

        private static Type FindFacetByFullName(string facetName, IEnumerable<Type> types)
        {
            // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
            return types
                .Where(t => t.FullName == facetName)
                .Where(t => typeof(Facet).IsAssignableFrom(t))
                .Where(t => t != typeof(Facet)) // except for the abstract class itself
                .FirstOrDefault();
        }
    }
}
