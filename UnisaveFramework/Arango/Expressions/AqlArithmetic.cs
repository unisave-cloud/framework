using System;
using LightJson;

namespace Unisave.Arango.Expressions
{
    // https://www.arangodb.com/docs/stable/aql/operators.html
    public static class AqlArithmetic
    {
        #region "Type casting"
        
        // https://www.arangodb.com/docs/stable/aql/functions-type-cast.html#to_bool
        public static bool ToBool(JsonValue value)
        {
            if (value.IsBoolean)
                return value.AsBoolean;
            
            if (value.IsNull)
                return false;
            
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (value.IsNumber)
                return value.AsNumber != 0.0;

            if (value.IsString)
                return value.AsString != "";

            if (value.IsJsonArray)
                return true;

            if (value.IsJsonObject)
                return true;
            
            // should not occur
            throw new ArgumentException(
                "Cannot convert JSON value to bool: " + value
            );
        }
        
        // https://www.arangodb.com/docs/stable/aql/functions-type-cast.html#to_number
        public static double ToNumber(JsonValue value)
        {
            if (value.IsNumber)
                return value;

            if (value.IsNull)
                return 0;

            if (value.IsBoolean)
                return value.AsBoolean ? 1 : 0;

            if (value.IsString)
            {
                string s = value.AsString.Trim();
                
                if (double.TryParse(s, out double result))
                    return result;

                return 0;
            }

            if (value.IsJsonArray)
            {
                JsonArray a = value.AsJsonArray;
                
                if (a.Count == 1)
                    return ToNumber(a[0]);

                return 0;
            }

            if (value.IsJsonObject)
                return 0;
            
            // should not occur
            throw new ArgumentException(
                "Cannot convert JSON value to number: " + value
            );
        }
        
        #endregion
        
        #region "Arithmetic operators (+ - * / %)"

        public static double Add(JsonValue a, JsonValue b)
            => ToNumber(a) + ToNumber(b);
        
        public static double Plus(JsonValue value)
            => ToNumber(value);
        
        public static double Subtract(JsonValue a, JsonValue b)
            => ToNumber(a) - ToNumber(b);
        
        public static double Minus(JsonValue value)
            => -ToNumber(value);
        
        public static double Multiply(JsonValue a, JsonValue b)
            => ToNumber(a) * ToNumber(b);
        
        public static double Divide(JsonValue a, JsonValue b)
            => ToNumber(a) / ToNumber(b);
        
        public static double Modulo(JsonValue a, JsonValue b)
            => ToNumber(a) % ToNumber(b);

        #endregion
        
        #region "Logical operators (NOT AND OR)"

        public static JsonValue And(JsonValue lhs, JsonValue rhs)
            => !ToBool(lhs) ? lhs : rhs;
        
        public static JsonValue Or(JsonValue lhs, JsonValue rhs)
            => ToBool(lhs) ? lhs : rhs;

        public static JsonValue Not(JsonValue value)
            => !ToBool(value);

        #endregion
        
        #region "Comparison operators (== != < <= > >=)"

        public static bool Equal(JsonValue lhs, JsonValue rhs)
        {
            // LightJson checks type equality and then value equality -
            // which is precisely what we need.
            return lhs == rhs;
        }

        public static bool NotEqual(JsonValue lhs, JsonValue rhs)
            => !Equal(lhs, rhs);

        public static bool LessThan(JsonValue lhs, JsonValue rhs)
        {
            // strings are special
            if (lhs.IsString || rhs.IsString)
                return string.CompareOrdinal(lhs, rhs) == -1;
            
            // else treat everything as a number
            return ToNumber(lhs) < ToNumber(rhs);
        }
        
        public static bool LessThanOrEqual(JsonValue lhs, JsonValue rhs)
            => LessThan(lhs, rhs) || Equal(lhs, rhs);

        public static bool GreaterThan(JsonValue lhs, JsonValue rhs)
            => LessThan(rhs, lhs);
        
        public static bool GreaterThenOrEqual(JsonValue lhs, JsonValue rhs)
            => GreaterThan(lhs, rhs) || Equal(lhs, rhs);

        #endregion
    }
}