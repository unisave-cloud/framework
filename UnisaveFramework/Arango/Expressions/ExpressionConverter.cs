using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LightJson;

namespace Unisave.Arango.Expressions
{
    /// <summary>
    /// Converts demungified LINQ expressions to AQL expressions
    /// </summary>
    public class ExpressionConverter
    {
        // used for JSON member access
        private readonly List<MethodInfo> possibleIndexerMethods;

        public ExpressionConverter()
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
        
        /// <summary>
        /// Main method
        /// </summary>
        public AqlExpression Convert(Expression expression)
        {
            return Visit(expression);
        }

        public AqlExpression Visit(Expression node)
        {
            switch (node)
            {
                case null:
                    return null;
                
                case ConstantExpression e:
                    return AqlConstantExpression.Create(e.Value);
                
                case ParameterExpression e:
                    return new AqlParameterExpression(e.Name);
                
                case UnaryExpression e:
                    return VisitUnary(e);
                
                case BinaryExpression e:
                    return VisitBinary(e);
                
                case MemberExpression e
                    when e.NodeType == ExpressionType.MemberAccess:
                    return VisitMemberAccess(e);
                
                case MethodCallExpression e
                    when e.NodeType == ExpressionType.Call:
                    return VisitCall(e);
                
                case NewArrayExpression e
                    when e.NodeType == ExpressionType.NewArrayInit:
                    return VisitNewArrayInit(e);
                
                case NewExpression e:
                    return VisitNew(e);
            }
            
            throw new AqlParsingException(
                $"Expression {node} cannot be converted to AQL expression"
            );
        }
        
        private AqlExpression VisitUnary(UnaryExpression node)
        {
            AqlExpression operand = Visit(node.Operand);

            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    // ignore the cast, because AQL does not support casts
                    return operand;
                
                case ExpressionType.UnaryPlus:
                    return new AqlUnaryOperator(
                        operand,
                        AqlExpressionType.UnaryPlus
                    );
                
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return new AqlUnaryOperator(
                        operand,
                        AqlExpressionType.UnaryMinus
                    );
                
                case ExpressionType.Not:
                    return new AqlUnaryOperator(
                        operand,
                        AqlExpressionType.Not
                    );
            }
            
            throw new AqlParsingException(
                $"Cannot parse node {node.NodeType} - not implemented"
            );
        }
        
        private AqlExpression VisitBinary(BinaryExpression node)
        {
            AqlExpression left = Visit(node.Left);
            AqlExpression right = Visit(node.Right);

            switch (node.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    if (node.Left.Type == typeof(string)
                        || node.Right.Type == typeof(string))
                    {
                        return new AqlFunctionExpression(
                            "CONCAT", left, right
                        );
                    }
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.Add
                    );
                
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.Subtract
                    );
                
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.Multiply
                    );
                
                case ExpressionType.Divide:
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.Divide
                    );
                
                case ExpressionType.Modulo:
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.Modulo
                    );
                
                case ExpressionType.Equal:
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.Equal
                    );
                
                case ExpressionType.NotEqual:
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.NotEqual
                    );
                
                case ExpressionType.GreaterThan:
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.GreaterThan
                    );
                
                case ExpressionType.GreaterThanOrEqual:
                    return new AqlBinaryOperator(
                        left, right,
                        AqlExpressionType.GreaterThanOrEqual
                    );
                
                case ExpressionType.LessThan:
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.LessThan
                    );
                
                case ExpressionType.LessThanOrEqual:
                    return new AqlBinaryOperator(
                        left, right,
                        AqlExpressionType.LessThanOrEqual
                    );
                
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.And
                    );
                
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return new AqlBinaryOperator(
                        left, right, AqlExpressionType.Or
                    );
            }
            
            throw new AqlParsingException(
                $"Cannot convert node {node.NodeType} - not implemented"
            );
        }
        
        private AqlExpression VisitMemberAccess(MemberExpression node)
        {
            // all possible member accesses have been simplified
            
            throw new AqlParsingException(
                "AQL cannot represent the following member " +
                "access operation: " + node
            );
            
            /*
             * Member access does not really exist in AQL, so it will be
             * always evaluated (simplified to constant), unless:
             * 1) indexer access (foo["bar"]) - handled by method call parsing
             * 2) parameter field access (x.bar) - throws exception
             * 3) entity field access (foo.bar) - actual member access expresion
             */
            
            // === first we need to get the instance containing the member ===
            
            object instance;
            
            if (node.Expression == null)
            {
                // we are accessing a static member
                instance = null;
            }
            else
            {
                var parsedExpression = Visit(node.Expression);
                
                // TODO handle entity member access
                
                //if (parsedExpression.CanSimplify)
                
                throw new AqlParsingException(
                    "AQL cannot represent the following member " +
                    "access operation: " + node
                );
                
//                instance = SimplifyParameterlessExpression(
//                    node.Expression
//                );
            }

            // === now we need to evaluate the member value itself ===
            
            object evaluatedMember;
                
            switch (node.Member)
            {
                case PropertyInfo pi:
                    evaluatedMember = pi.GetValue(instance);
                    break;
                
                case FieldInfo fi:
                    evaluatedMember = fi.GetValue(instance);
                    break;
                
                default:
                    throw new ArgumentException(
                        "Cannot parse member access to member type:"
                        + node.Member.MemberType
                    );
            }
            
            return AqlConstantExpression.Create(evaluatedMember);
        }
        
        private AqlExpression VisitCall(MethodCallExpression node)
        {
            AqlExpression instance = Visit(node.Object);
            List<AqlExpression> args = node.Arguments
                .Select(Visit)
                .ToList();

            // === handle arango functions ===
            
            ArangoFunctionAttribute attribute = node.Method
                .GetCustomAttribute<ArangoFunctionAttribute>();

            if (attribute != null)
                return new AqlFunctionExpression(attribute, args);
            
            // === intercept indexing on JSON objects ===

            if (possibleIndexerMethods.Contains(node.Method)
                && args.Count == 1)
            {
                return new AqlMemberAccessExpression(
                    instance,
                    args[0]
                );
            }
            
            // === handle JSON object construction ===
            
            // if we call .Add on a JsonObject instance
            if (node.Object?.Type == typeof(JsonObject)
                && node.Method.Name == "Add")
            {
                // start construction
                if (instance is AqlConstantExpression constant)
                    instance = new AqlJsonObjectExpression(constant.Value);
                
                // continue construction
                if (instance is AqlJsonObjectExpression obj)
                    return obj.CallAddViaArgs(args);
            }
            
            // === handle JSON array construction ===
            
            // if we call .Add on a JsonArray instance
            if (node.Object?.Type == typeof(JsonArray)
                && node.Method.Name == "Add")
            {
                // start construction
                if (instance is AqlConstantExpression constant)
                    instance = new AqlJsonArrayExpression(constant.Value);
                
                // continue construction
                if (instance is AqlJsonArrayExpression arr)
                    return arr.CallAddViaArgs(args);
            }

            // === expression is invalid ===
            
            throw new AqlParsingException(
                $"Expression uses parameters in method '{node.Method}'" +
                ", but this method cannot be translated to AQL"
            );
        }

        private AqlExpression VisitNewArrayInit(NewArrayExpression node)
        {
            // allow construction only of JsonValue[] arrays
            // (as an argument to the JsonArray constructor)
            if (node.Type.GetElementType() != typeof(JsonValue))
                throw new AqlParsingException(
                    $"Expression {node} cannot be converted to AQL expression"
                );
            
            List<AqlExpression> args = node.Expressions.Select(Visit).ToList();
            
            var result = new AqlJsonArrayExpression();

            foreach (AqlExpression item in args)
                result.Add(item);

            return result;
        }

        private AqlExpression VisitNew(NewExpression node)
        {
            // allow ONLY construction of JsonArray instances
            // (everything else fails)
            if (node.Type != typeof(JsonArray))
                throw new AqlParsingException(
                    $"Expression {node} cannot be converted to AQL expression"
                );
            
            if (node.Arguments.Count != 1)
                throw new Exception(
                    "JsonArray constructor is expected " +
                    "to have only one argument"
                );
            
            var jsonValueArrayArgument = Visit(node.Arguments[0]);
            
            if (!(jsonValueArrayArgument is AqlJsonArrayExpression))
                throw new Exception(
                    "Visiting JsonValue[] is expected to " +
                    "return AqlJsonArrayExpression"
                );

            return jsonValueArrayArgument;
        }
    }
}