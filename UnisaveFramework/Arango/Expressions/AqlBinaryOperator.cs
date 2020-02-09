using System;
using System.Collections.ObjectModel;
using System.Linq;
using LightJson;
using Unisave.Arango.Execution;

namespace Unisave.Arango.Expressions
{
    public class AqlBinaryOperator : AqlExpression
    {
        public override AqlExpressionType ExpressionType { get; }
        
        public override ReadOnlyCollection<string> Parameters { get; }
        
        /// <summary>
        /// Left operand
        /// </summary>
        public AqlExpression Left { get; }
        
        /// <summary>
        /// Right operand
        /// </summary>
        public AqlExpression Right { get; }
        
        public AqlBinaryOperator(
            AqlExpression left,
            AqlExpression right,
            AqlExpressionType type
        )
        {
            if (
                type != AqlExpressionType.Add
                && type != AqlExpressionType.Subtract
                && type != AqlExpressionType.Multiply
                && type != AqlExpressionType.Divide
                && type != AqlExpressionType.Modulo
                && type != AqlExpressionType.Equal
                && type != AqlExpressionType.NotEqual
                && type != AqlExpressionType.GreaterThan
                && type != AqlExpressionType.GreaterThanOrEqual
                && type != AqlExpressionType.LessThan
                && type != AqlExpressionType.LessThanOrEqual
                && type != AqlExpressionType.And
                && type != AqlExpressionType.Or
            )
                throw new ArgumentException(
                    $"Provided expression type {type} is not a binary operation"
                );
            
            ExpressionType = type;
            Left = left;
            Right = right;
            Parameters = new ReadOnlyCollection<string>(
                left.Parameters.Union(right.Parameters).ToList()
            );
        }
        
        public override string ToAql()
        {
            return $"({Left.ToAql()} {GetOperatorString()} {Right.ToAql()})";
        }

        private string GetOperatorString()
        {
            switch (ExpressionType)
            {
                case AqlExpressionType.Add: return "+";
                case AqlExpressionType.Subtract: return "-";
                case AqlExpressionType.Multiply: return "*";
                case AqlExpressionType.Divide: return "/";
                case AqlExpressionType.Modulo: return "%";
                case AqlExpressionType.Equal: return "==";
                case AqlExpressionType.NotEqual: return "!=";
                case AqlExpressionType.GreaterThan: return ">";
                case AqlExpressionType.GreaterThanOrEqual: return ">=";
                case AqlExpressionType.LessThan: return "<";
                case AqlExpressionType.LessThanOrEqual: return "<=";
                case AqlExpressionType.And: return "AND";
                case AqlExpressionType.Or: return "OR";
                
                default:
                    return ExpressionType.ToString();
            }
        }

        public override JsonValue EvaluateInFrame(ExecutionFrame frame)
        {
            JsonValue l = Left.EvaluateInFrame(frame);
            JsonValue r = Right.EvaluateInFrame(frame);
            
            switch (ExpressionType)
            {
                case AqlExpressionType.Add:
                    if (l.IsString)
                        return l.AsString + r;
                    if (r.IsString)
                        return l + r.AsString;
                    return l.AsNumber + r.AsNumber;
                
                case AqlExpressionType.Subtract: return l - r;
                case AqlExpressionType.Multiply: return l * r;
                // NOTE: double conversion really IS IMPORTANT
                case AqlExpressionType.Divide: return (double)l / (double)r;
                case AqlExpressionType.Modulo: return l % r;

                case AqlExpressionType.Equal: return l == r;
                case AqlExpressionType.NotEqual: return l != r;
                case AqlExpressionType.GreaterThan: return l > r;
                case AqlExpressionType.GreaterThanOrEqual: return l >= r;
                case AqlExpressionType.LessThan: return l < r;
                case AqlExpressionType.LessThanOrEqual: return l <= r;
                case AqlExpressionType.And: return l && r;
                case AqlExpressionType.Or: return l || r;
            }
            
            throw new QueryExecutionException(
                $"Cannot evaluate expression {ExpressionType} - not implemented"
            );
        }
    }
}