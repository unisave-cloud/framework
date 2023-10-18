using System;
using System.Collections;
using System.Collections.Generic;

namespace Unisave.Foundation
{
    /// <summary>
    /// A wrapper around a set of types, containing all the game backend types.
    /// This class can be resolved easily from the service container,
    /// whenever you need to go through all the backend types to register
    /// some behaviour (e.g. find facets, bootstrappers, etc...)
    /// </summary>
    public class BackendTypes : IEnumerable<Type>
    {
        private readonly HashSet<Type> types;

        public BackendTypes(IEnumerable<Type> types)
        {
            this.types = new HashSet<Type>(types);
        }
        
        public BackendTypes()
        {
            types = new HashSet<Type>();
        }

        public bool Add(Type type)
        {
            return types.Add(type);
        }
        
        public IEnumerator<Type> GetEnumerator()
        {
            return types.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}