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
        private static IArango GetArango()
        {
            return Facade.App.Resolve<IArango>();
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