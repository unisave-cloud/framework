using System;
using System.Collections.Generic;
using LightJson;
using Unisave.Arango.Query;
using Unisave.Contracts;

namespace Unisave.Facades
{
    /// <summary>
    /// Facade for executing AQL requests
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class AQL
    {
        /// <summary>
        /// Throws exception on client side with meaningful message
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        internal static void GuardClientSide()
        {
            if (!Facade.CanUse)
                throw new InvalidOperationException(
                    "You cannot access database from the client side. Make " +
                    "sure that the code shared between client and server " +
                    "does not access the database."
                );
        }
        
        private static IArango GetArango()
        {
            GuardClientSide();
            
            return Facade.Services.Resolve<IArango>();
        }
        
        /// <summary>
        /// Start a new AQL query execution
        /// </summary>
        public static AqlQuery Query() => new AqlQuery();

        /// <summary>
        /// Extension method to execute AQL queries directly
        /// </summary>
        public static IEnumerable<JsonValue> Execute(this AqlQuery query)
        {
            return GetArango().ExecuteAqlQuery(query);
        }
    }
}