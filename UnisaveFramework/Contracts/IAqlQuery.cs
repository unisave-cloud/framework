using System.Collections.Generic;
using LightJson;

namespace Unisave.Contracts
{
    public interface IAqlQuery
    {
        /// <summary>
        /// Sets value for a bind parameter
        /// </summary>
        /// <param name="parameter">Bind parameter name</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>The query itself to be chained</returns>
        IAqlQuery Bind(string parameter, JsonValue value);

        /// <summary>
        /// Executes the query, ignoring any results it might produce
        /// </summary>
        void Run();

        /// <summary>
        /// Executes the query and returns the result as a list of JSON values
        /// </summary>
        List<JsonValue> Get();
        
        /// <summary>
        /// Executes the query and deserializes the results to the
        /// type you specify
        /// </summary>
        List<TResult> GetAs<TResult>();
        
        /// <summary>
        /// Executes the query and returns only the first result as JSON.
        /// Returns null if no results were returned by the query.
        /// </summary>
        /// <returns></returns>
        JsonValue First();
        
        /// <summary>
        /// Executes the query and returns only the first result
        /// deserialized to the type you specify.
        /// Returns null if no results were returned by the query.
        /// </summary>
        TResult FirstAs<TResult>();
    }
}