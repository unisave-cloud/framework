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
            => ParseExpression(e.Body);
        
        public AqlExpression Parse(Expression<Func<JsonValue, JsonValue>> e)
            => ParseExpression(e.Body);
        
        public AqlExpression Parse(
            Expression<Func<JsonValue, JsonValue, JsonValue>> e
        ) => ParseExpression(e.Body);
        
        public AqlExpression Parse(
            Expression<Func<JsonValue, JsonValue, JsonValue, JsonValue>> e
        ) => ParseExpression(e.Body);

        /// <summary>
        /// Parses a generic expression
        /// </summary>
        public AqlExpression ParseExpression(Expression expression)
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
                
                case BinaryExpression e:
                    return ParseBinaryOperator(e);
                
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
            AqlExpression parsedOperand = ParseExpression(expression.Operand);

            // no parameters -> can be evaluated as a whole
            if (parsedOperand.CanSimplify)
                return SimplifyParameterlessExpression(expression);
            
            // has parameters -> ignore the cast, coz AQL does not support casts
            return parsedOperand;
        }

        private AqlExpression ParseUnaryOperator(UnaryExpression expression)
        {
            var parsedOperand = ParseExpression(expression.Operand);

            // evaluate, when not parametrized
            if (parsedOperand.CanSimplify)
                return SimplifyParameterlessExpression(expression);

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
        
        private AqlExpression ParseBinaryOperator(BinaryExpression expression)
        {
            var parsedLeft = ParseExpression(expression.Left);
            var parsedRight = ParseExpression(expression.Right);

            // evaluate, when not parametrized
            if (parsedLeft.CanSimplify && parsedRight.CanSimplify)
                return SimplifyParameterlessExpression(expression);

            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    if (expression.Left.Type == typeof(string)
                        || expression.Right.Type == typeof(string))
                    {
                        return new AqlFunctionExpression(
                            "CONCAT", parsedLeft, parsedRight
                        );
                    }
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.Add
                    );
                
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.Subtract
                    );
                
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.Multiply
                    );
                
                case ExpressionType.Divide:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.Divide
                    );
                
                case ExpressionType.Modulo:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.Modulo
                    );
                
                case ExpressionType.Equal:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.Equal
                    );
                
                case ExpressionType.NotEqual:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.NotEqual
                    );
                
                case ExpressionType.GreaterThan:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.GreaterThan
                    );
                
                case ExpressionType.GreaterThanOrEqual:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight,
                        AqlExpressionType.GreaterThanOrEqual
                    );
                
                case ExpressionType.LessThan:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.LessThan
                    );
                
                case ExpressionType.LessThanOrEqual:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight,
                        AqlExpressionType.LessThanOrEqual
                    );
                
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.And
                    );
                
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return new AqlBinaryOperator(
                        parsedLeft, parsedRight, AqlExpressionType.Or
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
                .Select(a => ParseExpression(a))
                .ToList();

            AqlExpression parsedObject = ParseExpression(expression.Object);

            bool parsedObjectCanSimplify = parsedObject?.CanSimplify
                                             ?? true;
            
            // === handle arango functions ===
            
            ArangoFunctionAttribute attribute = expression.Method
                .GetCustomAttribute<ArangoFunctionAttribute>();

            if (attribute != null)
                return new AqlFunctionExpression(attribute, parsedArguments);
            
            // === try to evaluate constant expression ===

            if (parsedArguments.All(a => a.CanSimplify)
                && parsedObjectCanSimplify)
            {
                return SimplifyParameterlessExpression(expression);
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
                if (parsedObjectCanSimplify)
                {
                    var nonParametrizedPart = SimplifyParameterlessExpression(
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
                .Select(a => ParseExpression(a))
                .ToList();
            
            if (parsedArguments.All(a => a.CanSimplify))
                return SimplifyParameterlessExpression(expression);
            
            throw new AqlParsingException(
                "Constructors (when parametrized) cannot be translated to " +
                "AQL, in expresion " + expression
            );
        }

        /// <summary>
        /// If an expression contains no parameters, it can be evaluated
        /// to a constant.
        /// </summary>
        private AqlConstantExpression SimplifyParameterlessExpression(
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