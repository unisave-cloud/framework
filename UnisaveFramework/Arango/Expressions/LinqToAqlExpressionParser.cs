using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LightJson;

namespace Unisave.Arango.Expressions
{
    /// <summary>
    /// Converts LINQ expression trees to AQL expression trees
    /// </summary>
    public class LinqToAqlExpressionParser
    {
        // used for JSON member access
        private readonly List<MethodInfo> possibleIndexerMethods;
        
        public LinqToAqlExpressionParser()
        {
            possibleIndexerMethods = new[] {
                    typeof(JsonValue),
                    typeof(JsonObject),
                    typeof(JsonArray)
                }
                .SelectMany(t => t.GetProperties())
                .Where(p => p.Name == "Item" && p.GetIndexParameters().Length == 1)
                .Select(p => p.GetMethod)
                .ToList();
        }
        
        public AqlExpression Parse(Expression<Func<JsonValue>> e)
            => Parse(e.Body);
        
        public AqlExpression Parse(Expression<Func<JsonValue, JsonValue>> e)
            => Parse(e.Body);
        
        public AqlExpression Parse(
            Expression<Func<JsonValue, JsonValue, JsonValue>> e
        ) => Parse(e.Body);
        
        public AqlExpression Parse(
            Expression<Func<JsonValue, JsonValue, JsonValue, JsonValue>> e
        ) => Parse(e.Body);

        protected AqlExpression Parse(Expression expression)
        {
            switch (expression)
            {
                case null:
                    return null;
                
                case UnaryExpression e
                    when e.NodeType == ExpressionType.Convert:
                    return ParseConversion(e);
                
                case UnaryExpression e:
                    return ParseUnaryOperator(e);
                
                case ConstantExpression e:
                    return AqlConstantExpression.Create(e.Value);
                
                case ParameterExpression e
                    when e.NodeType == ExpressionType.Parameter:
                    return new AqlParameterExpression(e.Name);
                
                case MemberExpression e
                    when e.NodeType == ExpressionType.MemberAccess:
                    return ParseMemberAccess(e);
                
                case MethodCallExpression e
                    when e.NodeType == ExpressionType.Call:
                    return ParseMethodCall(e);
                
                case NewExpression e
                    when e.NodeType == ExpressionType.New:
                    return ParseConstructorCall(e);
            }
            
            throw new AqlParsingException(
                $"Cannot parse expression {expression} - not implemented"
            );
        }

        private AqlExpression ParseConversion(UnaryExpression expression)
        {
            AqlExpression parsedOperand = Parse(expression.Operand);

            // no parameters -> can be evaluated as a whole
            if (!parsedOperand.HasParameters)
                return EvaluateParameterlessExpression(expression);
            
            // has parameters -> ignore the cast, coz AQL does not support casts
            return parsedOperand;
        }

        private AqlExpression ParseUnaryOperator(UnaryExpression expression)
        {
            var parsedOperand = Parse(expression.Operand);

            // evaluate, when not parametrized
            if (!parsedOperand.HasParameters)
                return EvaluateParameterlessExpression(expression);

            switch (expression.NodeType)
            {
                case ExpressionType.UnaryPlus:
                    return new AqlUnaryOperator(
                        parsedOperand,
                        AqlExpressionType.UnaryPlus
                    );
                
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return new AqlUnaryOperator(
                        parsedOperand,
                        AqlExpressionType.UnaryMinus
                    );
                
                case ExpressionType.Not:
                    return new AqlUnaryOperator(
                        parsedOperand,
                        AqlExpressionType.Not
                    );
            }
            
            throw new AqlParsingException(
                $"Cannot parse node {expression.NodeType} - not implemented"
            );
        }

        private AqlExpression ParseMemberAccess(MemberExpression expression)
        {
            object instance = null;
            
            if (expression.Expression != null)
            {
                // TODO: if member expressing has no parameters, only then eval
                // + Parse(expression.Expression);
                instance = Expression
                    .Lambda(expression.Expression)
                    .Compile()
                    .DynamicInvoke();
            }

            switch (expression.Member)
            {
                case PropertyInfo pi:
                    return AqlConstantExpression.Create(pi.GetValue(instance));
                
                case FieldInfo fi:
                    return AqlConstantExpression.Create(fi.GetValue(instance));
                
                default:
                    throw new ArgumentException(
                        "Cannot parse member access to member type:"
                        + expression.Member.MemberType
                    );
            }
        }

        private AqlExpression ParseMethodCall(MethodCallExpression expression)
        {
            List<AqlExpression> parsedArguments = expression.Arguments
                .Select(a => Parse(a))
                .ToList();

            AqlExpression parsedObject = Parse(expression.Object);

            bool parsedObjectHasParameters = parsedObject?.HasParameters
                                             ?? false;
            
            // === handle arango functions ===
            
            ArangoFunctionAttribute attribute = expression.Method
                .GetCustomAttribute<ArangoFunctionAttribute>();

            if (attribute != null)
                return new AqlFunctionExpression(attribute, parsedArguments);
            
            // === try to evaluate constant expression ===

            if (parsedArguments.All(a => !a.HasParameters)
                && !parsedObjectHasParameters)
            {
                return EvaluateParameterlessExpression(expression);
            }
            
            // === intercept indexing on JSON objects ===

            if (possibleIndexerMethods.Contains(expression.Method)
                && parsedArguments.Count == 1)
            {
                return new AqlMemberAccessExpression(
                    parsedObject,
                    parsedArguments[0]
                );
            }
            
            // === handle JSON object construction ===

            if (expression.Object?.Type == typeof(JsonObject)
                && expression.Method.Name == "Add")
            {
                // continue construction
                if (parsedObject is AqlJsonObjectExpression obj)
                    return obj.Add(parsedArguments);
                
                // start new construction
                if (!parsedObjectHasParameters)
                {
                    var nonParametrizedPart = EvaluateParameterlessExpression(
                        expression.Object
                    ).Value.AsJsonObject;
                    
                    var newObj = new AqlJsonObjectExpression(
                        nonParametrizedPart
                    );
                    
                    return newObj.Add(parsedArguments);
                }
            }

            // === expression is invalid ===
            
            throw new AqlParsingException(
                $"Expression uses parameters in method '{expression.Method}'" +
                ", but this function cannot be translated to AQL"
            );
        }

        private AqlExpression ParseConstructorCall(NewExpression expression)
        {
            List<AqlExpression> parsedArguments = expression.Arguments
                .Select(a => Parse(a))
                .ToList();
            
            if (parsedArguments.All(a => !a.HasParameters))
                return EvaluateParameterlessExpression(expression);
            
            throw new AqlParsingException(
                "Constructors (when parametrized) cannot be translated to " +
                "AQL, in expresion " + expression
            );
        }

        /// <summary>
        /// If an expression contains no parameters, it can be evaluated
        /// to a constant.
        /// </summary>
        private AqlConstantExpression EvaluateParameterlessExpression(
            Expression expression
        )
        {
            LambdaExpression lambdaExpression = Expression.Lambda(expression);
            Delegate compiledLambda = lambdaExpression.Compile();
            object result = compiledLambda.DynamicInvoke();
            return AqlConstantExpression.Create(result);
        }
    }
}