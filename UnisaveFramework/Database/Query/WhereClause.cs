using LightJson;
using Unisave.Exceptions;

namespace Unisave.Database.Query
{
    /// <summary>
    /// Represents a where clause in an entity query
    ///
    /// Every instance of a where clause should be immutable
    /// </summary>
    public abstract class WhereClause
    {
        /// <summary>
        /// Returns the type of the where clause
        /// </summary>
        public abstract string ClauseType { get; }
        
        /// <summary>
        /// How is this clause logically joint with the previous clauses
        /// Has no impact on the first clause
        /// </summary>
        public enum BooleanValue
        {
            And,
            Or
        }

        /// <summary>
        /// Boolean type of this where clause (AND or OR)
        /// </summary>
        public BooleanValue Boolean
        {
            get => boolean == "and" ? BooleanValue.And : BooleanValue.Or;
            private set => boolean = (value == BooleanValue.And) ? "and" : "or";
        }

        // backing field
        private string boolean = "and";
        
        public WhereClause(BooleanValue boolean)
        {
            Boolean = boolean;
        }

        /// <summary>
        /// Deserializes WHERE clause from JSON
        /// </summary>
        public static WhereClause FromJson(JsonValue jsonValue)
        {
            JsonObject json = jsonValue.AsJsonObject;

            string type = json["type"].AsString;
            BooleanValue boolean = json["boolean"].AsString?.ToLower() == "and"
                ? BooleanValue.And
                : BooleanValue.Or;
            
            switch (type)
            {
                case BasicWhereClause.BasicClauseType:
                    return BasicWhereClause.FromJson(json, boolean);
                
                default:
                    throw new UnisaveException(
                        $"Where clause of unknown type '{type}'."
                    );
            }
        }

        /// <summary>
        /// Serializes the WHERE clause into its JSON representation
        /// </summary>
        public JsonObject ToJson()
        {
            var json = new JsonObject();

            ToJson(json);
            
            json.Add("boolean", boolean);
            json.Add("type", ClauseType);

            return json;
        }

        /// <summary>
        /// Append additional data to a json object representing the clause
        /// </summary>
        protected abstract void ToJson(JsonObject json);
    }
}