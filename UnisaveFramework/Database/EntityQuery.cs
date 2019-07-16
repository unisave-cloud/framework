using System;
using System.Collections.Generic;

namespace Unisave.Database
{
    /// <summary>
    /// Describes a specific subset of entities of a given type
    /// </summary>
    public class EntityQuery
    {
        /// <summary>
        /// What owners do we require the entity to have
        /// </summary>
        public ISet<UnisavePlayer> requiredOwners = new HashSet<UnisavePlayer>();

        /// <summary>
        /// Is the set of required owners exact, or can it be just a subset of the actual owners?
        /// </summary>
        public bool requireOwnersExactly = true;

        /// <summary>
        /// Take the first found entity and return only that
        /// </summary>
        public bool takeFirstFound = false;
    }
}
