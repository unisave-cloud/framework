using System;
using System.Reflection;

namespace Unisave
{
    public abstract class Facet
    {
        /// <summary>
        /// The player who called this facet
        /// </summary>
        protected UnisavePlayer Caller { get; private set; }

        public static Facet CreateInstance(Type facetType, UnisavePlayer caller)
        {
            if (!typeof(Facet).IsAssignableFrom(facetType))
                throw new ArgumentException(
                    "Provided type does not inherit from the Facet class.",
                    nameof(facetType)
                );

            ConstructorInfo ci = facetType.GetConstructor(new Type[] { });

            if (ci == null)
            {
                throw new ArgumentException(
                    "Provided facet type " + facetType + " lacks empty constructor.",
                    nameof(facetType)
                );
            }

            Facet facet = (Facet)ci.Invoke(new object[] { });

            facet.Caller = caller;

            return facet;
        }
    }
}
