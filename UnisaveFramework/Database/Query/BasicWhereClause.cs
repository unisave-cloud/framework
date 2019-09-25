using System;
using LightJson;
using System.Linq;

namespace Unisave.Database.Query
{
    /// <summary>
    /// Where clause comparing values to a constant
    /// </summary>
    public class BasicWhereClause : WhereClause
    {
        /// <summary>
        /// Type of the clause used for serialization
        /// </summary>
        public const string BasicClauseType = "Basic";

        /// <inheritdoc/>
        public override string ClauseType => BasicClauseType;

        /// <summary>
        /// Path to the value to be compared
        /// </summary>
        public JsonPath Path { get; }
        
        /// <summary>
        /// Comparison operator
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// Constant value that is used for comparison
        /// </summary>
        public JsonValue Value { get; }

        public BasicWhereClause(
            JsonPath path,
            string op,
            JsonValue value,
            BooleanValue boolean = BooleanValue.And
        ) : base(boolean)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Value = value;

            Operator = PreprocessOperator(Operator);
            
            ValidateOperator(Operator);
        }

        /// <summary>
        /// Deserializes WHERE clause from JSON
        /// </summary>
        public static BasicWhereClause FromJson(
            JsonObject json, BooleanValue boolean
        )
        {
            return new BasicWhereClause(
                JsonPath.FromJson(json["path"]),
                json["operator"].AsString,
                json["value"],
                boolean
            );
        }

        /// <inheritdoc/>
        protected override void ToJson(JsonObject json)
        {
            json.Add("path", Path.ToJson());
            json.Add("operator", Operator);
            json.Add("value", Value);
        }

        /// <summary>
        /// Does some operator normalization that tries to save
        /// developer sanity
        /// </summary>
        public static string PreprocessOperator(string op)
        {
            if (op == "==")
                return "=";

            return op;
        }

        /// <summary>
        /// Verifies that an operator is a valid one,
        /// throws argument exception otherwise
        /// </summary>
        public static void ValidateOperator(string op)
        {
            string[] allowed = {
                "=", "<", ">", "<=", ">=", "<>", "!="
            };

            if (!allowed.Contains(op))
                throw new ArgumentException(
                    $"Given where clause operator '{op}' is not valid."
                );
        }
    }
}